using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Test.IntegrationTests;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SpecsFor.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Bonobo.Git.Server.Test.Integration.ClAndWeb
{
    public class GitInstance
    {
        public string GitExe { get; set; }
        public MsysgitResources Resources { get; set; }
    }

    /// <summary>
    /// This is a regression test for msysgit clients. It can be run against installed version of Bonobo Git Server.
    /// </summary>
    [TestClass]
    public class MsysgitIntegrationTests
    {
        private readonly static string RepositoryName = "Integration";
        private readonly static string RepositoryUrlTemplate = "http://{0}localhost:20000/{2}{1}";

        private readonly static string WorkingDirectory = Path.GetFullPath(@"..\..\..\Tests\IntegrationTests");
        private readonly static string GitPath = Path.GetFullPath(@"..\..\..\Gits\{0}\bin\git.exe");

        private readonly static string ServerRepositoryPath = Path.Combine(@"..\..\..\Bonobo.Git.Server\App_Data\Repositories", RepositoryName);
        private readonly static string RepositoryDirectory = Path.Combine(WorkingDirectory, RepositoryName);
        private readonly static string ServerRepositoryBackupPath = Path.Combine(@"..\..\..\Tests\", RepositoryName, "Backup");

        private static string RepositoryUrlWithCredentials;
        private static string RepositoryUrlWithoutCredentials;
        private static string Url;
        private static string BareUrl;

        private static string AdminCredentials;
        private static string UserCredentials;

        private readonly static string[] GitVersions = { "1.7.4", "1.7.6", "1.7.7.1", "1.7.8", "1.7.9", "1.8.0", "1.8.1.2", "1.8.3", "1.9.5", "2.6.1" };
        private static List<GitInstance> installedgits = new List<GitInstance>();

        private static MvcWebApp app;
        private static IntegrationTestHelpers ITH;
        private static LoadedConfig lc;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            // Make sure relative paths are frozen in case the app's CurrentDir changes
            // WorkingDirectory = Path.GetFullPath(WorkingDirectory);
            // GitPath = Path.GetFullPath(GitPath);
            // RepositoryDirectory = Path.Combine(WorkingDirectory, RepositoryName);
            
            List<string> not_found = new List<string>();

            foreach (var version in GitVersions)
            {
                var git = String.Format(GitPath, version);
                if (File.Exists(git))
                {
                    installedgits.Add(new GitInstance { GitExe =  git, Resources =  new MsysgitResources(version) });
                }
                else
                {
                    not_found.Add(git);
                }
            }

            if (!installedgits.Any())
            {
                Assert.Fail(string.Format("Please ensure that you have at least one git installation in '{0}'.", string.Join("', '", not_found.Select(n => Path.GetFullPath(n)))));
            }

            lc = AssemblyStartup.LoadedConfig;

            AdminCredentials = lc.getUrlLogin("admin") + "@";
            UserCredentials = lc.getUrlLogin("user") + "@";

            Directory.CreateDirectory(WorkingDirectory);

            if (AnyCredentialHelperExists(installedgits.Last()))
            {
                /* At the moment there is no reliable way of overriding credential.helper on a global basis.
                 * See the other comments for all the other bugs found so far.
                 * Having a credential helper set makes it impossible to check in non authorized login
                 * after a login with username and password has been done. */
                Assert.Fail("Cannot have any credential.helpers configured for integration tests.");
            }

            app = new MvcWebApp();
            ITH = new IntegrationTestHelpers(app, lc);

            RepositoryUrlWithCredentials = String.Format(RepositoryUrlTemplate, AdminCredentials, ".git", RepositoryName);
            RepositoryUrlWithoutCredentials = String.Format(RepositoryUrlTemplate, String.Empty, String.Empty, RepositoryName);
            Url = string.Format(RepositoryUrlTemplate, string.Empty, string.Empty, string.Empty);
            BareUrl = Url.TrimEnd('/');
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            app.Browser.Close();
        }

        [TestInitialize]
        public void Initialize()
        {
            ITH.DeleteDirectory(WorkingDirectory);
            ITH.LoginAndResetDatabase();
        }

        [TestMethod, TestCategory(TestCategories.IntegrationTest)]
        public void RunGitTests()
        {

            ForAllGits(git =>
                {
                    Guid repo_id = ITH.CreateRepositoryOnWebInterface(RepositoryName);
                    CloneEmptyRepositoryWithCredentials(git);
                    CreateIdentity(git);
                    CreateAndPushFiles(git);
                    PushTag(git);
                    PushBranch(git);

                    ITH.DeleteDirectory(RepositoryDirectory);
                    CloneRepository(git);

                    ITH.DeleteDirectory(RepositoryDirectory);
                    Directory.CreateDirectory(RepositoryDirectory);
                    InitAndPullRepository(git);
                    PullTag(git);
                    PullBranch(git);
                });
        }

        [TestMethod, TestCategory(TestCategories.IntegrationTest)]
        public void AnonRepoClone()
        {
            /* This test can fail if you have any credential.helper setup on your system.
             * This is because it is a multi-value configuration which cannot (yet) be
             * unset on the command line or any other way. I have reported it and
             * maybe a new git version will be able to do it.
             * See http://article.gmane.org/gmane.comp.version-control.git/287538
             * and patch status http://article.gmane.org/gmane.comp.version-control.git/287565
             */
            ForAllGits(git =>
            {
                Guid repo_id = ITH.CreateRepositoryOnWebInterface(RepositoryName);
                AllowAnonRepoClone(repo_id, false);
                CloneRepoAnon(git, false);
                AllowAnonRepoClone(repo_id, true);
                CloneRepoAnon(git, true);
            });
        }

        private static bool AnyCredentialHelperExists(GitInstance git)
        {
            IEnumerable<string> urls = new List<string>
            {
                string.Format(RepositoryUrlTemplate, string.Empty, string.Empty, string.Empty),
                string.Format(RepositoryUrlTemplate, AdminCredentials, string.Empty, string.Empty),
            };

            foreach (var url in urls)
            {
                var exists = RunGit(git, string.Format("config --get-urlmatch credential.helper {0}", url), WorkingDirectory);
                /* Credential.helper is a multi-valued configuration setting. This means it you cannot rely on the value returned by any of the calls
                 * as it might return an empty value, which is a valid(! altough pointless) value where there is a second configured value
                 * in the same config file. But you can rely on the exit code. 0 means it found this key. */
                /* There seems to be a bug in git config --get-urlmatch where it will retun 0 eventhough there was no match.
                 * So we need to check the value. The good part is that if it is not set anywhere we get an empty string. If
                 * it is set somewhere the string contains at least "\r\n".
                 * See the bug report here http://article.gmane.org/gmane.comp.version-control.git/287740 */
                if (exists.ExitCode == 0 && exists.StdOut != "")
                {
                    Console.Write(string.Format("Stdout: {0}", exists.StdOut));
                    Console.Write(string.Format("Stderr: {0}", exists.StdErr));
                    Debug.Write(string.Format("Stdout: {0}", exists.StdOut));
                    Debug.Write(string.Format("Stderr: {0}", exists.StdErr));
                    return true;
                }
            }

            return false;
        }

        [TestMethod, TestCategory(TestCategories.IntegrationTest)]
        public void NoDeadlockOnLargeOutput()
        {
            var git = installedgits.Last();
            Directory.CreateDirectory(WorkingDirectory);

                var repo_id = ITH.CreateRepositoryOnWebInterface(RepositoryName);
                CloneEmptyRepositoryWithCredentials(git);
                CreateIdentity(git);
                CreateAndAddTestFiles(git, 2000);
        }

        [TestMethod, TestCategory(TestCategories.IntegrationTest)]
        public void RepoAnonPushRespectsGlobalSettings()
        {

            ForAllGits(git =>
            {
                var repo_id = ITH.CreateRepositoryOnWebInterface(RepositoryName);
                AllowAnonRepoClone(repo_id, true);
                CloneRepoAnon(git, true);
                CreateIdentity(git);
                SetRepoPushTo(repo_id, RepositoryPushMode.Global);

                CreateRandomFile(Path.Combine(RepositoryDirectory, "file.txt"), 0);
                RunGitOnRepo(git, "add .").ExpectSuccess();
                RunGitOnRepo(git, "commit -m\"Aw yeah!\"").ExpectSuccess();

                SetAnonPush(false);
                PushFiles(git, false);

                SetAnonPush(true);
                PushFiles(git, true);
            });
        }

        [TestMethod, TestCategory(TestCategories.IntegrationTest)]
        public void RepoAnonPushYesOverridesGlobalSettings()
        {

            ForAllGits(git =>
            {
                var repo_id = ITH.CreateRepositoryOnWebInterface(RepositoryName);
                AllowAnonRepoClone(repo_id, true);
                CloneRepoAnon(git, true);
                CreateIdentity(git);
                SetRepoPushTo(repo_id, RepositoryPushMode.Yes);

                CreateRandomFile(Path.Combine(RepositoryDirectory, "file.txt"), 0);
                RunGitOnRepo(git, "add .");
                RunGitOnRepo(git, "commit -m\"Aw yeah!\"");

                SetGlobalAnonPush(git, false);
                PushFiles(git, true);
            });
        }

        [TestMethod, TestCategory(TestCategories.IntegrationTest)]
        public void RepoAnonPushNoOverridesGlobalSettings()
        {

            ForAllGits(git =>
            {
                var repo_id = ITH.CreateRepositoryOnWebInterface(RepositoryName);
                AllowAnonRepoClone(repo_id, true);
                CloneRepoAnon(git, true);
                CreateIdentity(git);
                SetRepoPushTo(repo_id, RepositoryPushMode.No);

                CreateRandomFile(Path.Combine(RepositoryDirectory, "file.txt"), 0);
                RunGitOnRepo(git, "add .");
                RunGitOnRepo(git, "commit -m\"Aw yeah!\"");

                SetGlobalAnonPush(git, true);
                PushFiles(git, false);
            });
        }

        private void SetRepoPushTo(Guid repo_id, RepositoryPushMode repositoryPushStatus)
        {
            app.NavigateTo<RepositoryController>(c => c.Edit(repo_id));
            var form = app.FindFormFor<RepositoryDetailModel>();
            var select = new SelectElement(form.Field(f => f.AllowAnonymousPush).Field);
            select.SelectByValue(repositoryPushStatus.ToString("D"));
            form.Submit();
            ITH.AssertThatNoValidationErrorOccurred();
        }

        /// <summary>
        /// This does an authorized push to a repo that allows anon clone
        /// At the time of writing, this demonstrates an issue which breaks authorised push if the repo allows anon pull
        /// </summary>
        [TestMethod, TestCategory(TestCategories.IntegrationTest)]
        public void NamedPushToAnonRepo()
        {
            ForAllGits(git =>
            {
                Guid repo_id = ITH.CreateRepositoryOnWebInterface(RepositoryName);

                // Clone the repo
                AllowAnonRepoClone(repo_id, true);
                CloneEmptyRepositoryWithCredentials(git);

                CreateIdentity(git);
                // I want to do a push *with* a username
                CreateAndPushFiles(git);
            });
        }
        [TestMethod, TestCategory(TestCategories.IntegrationTest)]
        public void PushToCreateIsNotNormallyAllowed()
        {
            ForAllGits(git =>
            {
                // Create a repo locally
                Directory.CreateDirectory(RepositoryDirectory);
                InitRepository(git);
                Environment.CurrentDirectory = RepositoryDirectory;
                CreateIdentity(git);
                CreateAndAddFiles(git);

                RunGitOnRepo(git, "push origin master").ErrorMustMatch(MsysgitResources.Definition.RepositoryNotFoundError, RepositoryUrlWithCredentials);
            });
        }

        [TestMethod, TestCategory(TestCategories.IntegrationTest)]
        public void PushToCreateIsAllowedIfOptionIsSet()
        {
            ForAllGits(git =>
            {
                // Enable the push-to-create option
                ITH.SetGlobalSetting(x => x.AllowPushToCreate, true);

                // Create a repo locally
                Directory.CreateDirectory(RepositoryDirectory);
                InitRepository(git);
                Environment.CurrentDirectory = RepositoryDirectory;
                CreateIdentity(git);
                CreateAndAddFiles(git);

                RunGitOnRepo(git, "push origin master").ExpectSuccess();

                // Ensure repo is created with same name as was pushed
                Guid repoId = ITH.FindRepository(RepositoryName);
                Assert.AreNotEqual(Guid.Empty, repoId);
            });
        }

        [TestMethod, TestCategory(TestCategories.IntegrationTest)]
        public void LinkifyGlobalWorks()
        {

            ForAllGits(git =>
            {

                var repo_id = ITH.CreateRepositoryOnWebInterface(RepositoryName);
                ITH.SetGlobalSetting(m => m.LinksRegex, @"#(\d)(\d+)");
                ITH.SetGlobalSetting(m => m.LinksUrl, @"http://some.url/{0}{1}{2}");
                app.NavigateTo<RepositoryController>(c => c.Edit(repo_id));
                var form = app.FindFormFor<RepositoryDetailModel>();
                ITH.SetCheckbox(form.Field(f => f.LinksUseGlobal).Field, true);
                form.Submit();


                CloneEmptyRepositoryWithCredentials(git);
                CreateIdentity(git);
                CreateAndAddTestFiles(git, 1);
                RunGitOnRepo(git, "push origin master").ExpectSuccess();

                app.NavigateTo<RepositoryController>(c => c.Commits(repo_id, null, 1));
                var display = app.FindDisplayFor<RepositoryCommitsModel>();
                var links = app.Browser.FindElementsByCssSelector("a.linkified");
                foreach (var link in links)
                {
                    Assert.AreEqual("http://some.url/#12341234", link.GetAttribute("href"));
                }

                ITH.DeleteRepositoryUsingWebsite(repo_id);
            });

        }

        [TestMethod, TestCategory(TestCategories.IntegrationTest)]
        public void LinkifyRepoOverridesGlobal()
        {

            ForAllGits(git =>
            {

                ITH.SetGlobalSetting(m => m.LinksRegex, @"#(\d)(\d+)");
                ITH.SetGlobalSetting(m => m.LinksUrl, @"http://some.url/{0}{1}{2}");

                var repo_id = ITH.CreateRepositoryOnWebInterface(RepositoryName);

                app.NavigateTo<RepositoryController>(c => c.Edit(repo_id));
                var form = app.FindFormFor<RepositoryDetailModel>();
                ITH.SetCheckbox(form.Field(f => f.LinksUseGlobal).Field, false);
                form.Field(f => f.LinksRegex).SetValueTo(@"#\d+");
                form.Field(f => f.LinksUrl).SetValueTo(@"http://otherurl.here/{0}");
                form.Submit();


                CloneEmptyRepositoryWithCredentials(git);
                CreateIdentity(git);
                CreateAndAddTestFiles(git, 1);
                RunGitOnRepo(git, "push origin master").ExpectSuccess();

                app.NavigateTo<RepositoryController>(c => c.Commits(repo_id, null, 1));
                var display = app.FindDisplayFor<RepositoryCommitsModel>();
                var links = app.Browser.FindElementsByCssSelector("a.linkified");
                foreach (var link in links)
                {
                    Assert.AreEqual("http://otherurl.here/#1234", link.GetAttribute("href"));
                }

                ITH.DeleteRepositoryUsingWebsite(repo_id);
            });

        }
        [TestMethod, TestCategory(TestCategories.IntegrationTest)]
        public void LinkifyRepoRegexEmptyGeneratesNoLinks()
        {

            ForAllGits(git =>
            {

                ITH.SetGlobalSetting(m => m.LinksRegex, @"#(\d)(\d+)");
                ITH.SetGlobalSetting(m => m.LinksUrl, @"http://some.url/{0}{1}{2}");

                var repo_id = ITH.CreateRepositoryOnWebInterface(RepositoryName);

                app.NavigateTo<RepositoryController>(c => c.Edit(repo_id));
                var form = app.FindFormFor<RepositoryDetailModel>();
                ITH.SetCheckbox(form.Field(f => f.LinksUseGlobal).Field, false);
                form.Field(f => f.LinksRegex).SetValueTo("");
                form.Field(f => f.LinksUrl).SetValueTo("");
                form.Submit();


                CloneEmptyRepositoryWithCredentials(git);
                CreateIdentity(git);
                CreateAndAddTestFiles(git, 1);
                RunGitOnRepo(git, "push origin master").ExpectSuccess();

                app.NavigateTo<RepositoryController>(c => c.Commits(repo_id, null, 1));
                var display = app.FindDisplayFor<RepositoryCommitsModel>();
                var links = app.Browser.FindElementsByCssSelector("a.linkified");
                Assert.AreEqual(0, links.Count);
                ITH.DeleteRepositoryUsingWebsite(repo_id);
            });

        }

        [TestMethod, TestCategory(TestCategories.IntegrationTest)]
        public void CheckTagLinksWorkInViews()
        {
            ForAllGits(git =>
            {
                var repo_id = ITH.CreateRepositoryOnWebInterface(RepositoryName);
                CloneEmptyRepositoryWithCredentials(git);
                CreateIdentity(git);
                CreateAndAddFiles(git);
                RunGitOnRepo(git, "push origin master").ExpectSuccess();
                RunGitOnRepo(git, "tag a HEAD").ExpectSuccess();
                RunGitOnRepo(git, "push --tags").ExpectSuccess();

                var gitresult = RunGitOnRepo(git, "rev-parse HEAD").ExpectSuccess();
                var commit_id = gitresult.StdOut.TrimEnd();
                // check link in Commits
                app.NavigateTo<RepositoryController>(c => c.Commits(repo_id, string.Empty, 1));
                var link = app.Browser.FindElementByCssSelector("span.tag a");
                link.Click();
                
                app.UrlShouldMapTo<RepositoryController>(c => c.Commits(repo_id, "a", 1)); 

                // check link in tags
                app.NavigateTo<RepositoryController>(c => c.Tags(repo_id, string.Empty, 1));
                link = app.Browser.FindElementByCssSelector("div.tag a");
                link.Click();
                app.UrlShouldMapTo<RepositoryController>(c => c.Commits(repo_id, "a", 1)); 

                // check link in single commit
                app.NavigateTo<RepositoryController>(c => c.Commit(repo_id, commit_id));
                link = app.Browser.FindElementByCssSelector("span.tag a");
                link.Click();
                app.UrlShouldMapTo<RepositoryController>(c => c.Commits(repo_id, "a", 1));

                ITH.DeleteRepositoryUsingWebsite(repo_id);
            });
        }
        
        [TestMethod, TestCategory(TestCategories.IntegrationTest)]
        public void CanNavigateIntoBranchesFolder()
        {
            ForAllGits(git =>
            {
                var repo = ITH.CreateRepositoryOnWebInterface(RepositoryName);
                CloneEmptyRepositoryWithCredentials(git);
                CreateIdentity(git);
                CreateAndAddTestFiles(git, 1);
                RunGitOnRepo(git, "branch branchX");
                Directory.CreateDirectory(Path.Combine(RepositoryDirectory, "dir1"));
                File.Create(Path.Combine(RepositoryDirectory, "dir1", "file1.txt")).Close();
                RunGitOnRepo(git, "add dir1").ExpectSuccess();
                RunGitOnRepo(git, "commit -m\"dir1 on master\"").ExpectSuccess();
                RunGitOnRepo(git, "push --set-upstream origin master").ExpectSuccess();
                RunGitOnRepo(git, "checkout branchX").ExpectSuccess();
                Directory.CreateDirectory(Path.Combine(RepositoryDirectory, "dir2"));
                File.Create(Path.Combine(RepositoryDirectory, "dir2", "file2.txt")).Close();
                RunGitOnRepo(git, "add dir2").ExpectSuccess();
                RunGitOnRepo(git, "commit -m\"dir2 on branchX\"").ExpectSuccess();
                RunGitOnRepo(git, "push --set-upstream origin branchX").ExpectSuccess();

                app.NavigateTo<RepositoryController>(c => c.Tree(repo, null, null));
                var elements = app.Browser.FindElementsByCssSelector("table#files td.path a.directory");
                Assert.AreEqual(1, elements.Count);
                Assert.AreEqual("dir1", elements[0].Text);
                elements[0].Click();
                app.WaitForElementToBeVisible(By.CssSelector("nav.branches"), TimeSpan.FromSeconds(1));
                app.UrlShouldMapTo<RepositoryController>(c => c.Tree(repo, null, "dir1"));

                app.NavigateTo<RepositoryController>(c => c.Tree(repo, "branchX", null));
                app.WaitForElementToBeVisible(By.CssSelector("nav.branches"), TimeSpan.FromSeconds(1));
                app.UrlShouldMapTo<RepositoryController>(c => c.Tree(repo, "branchX", null));
                elements = app.Browser.FindElementsByCssSelector("table#files td.path a.directory");
                Assert.AreEqual(1, elements.Count);
                Assert.AreEqual("dir2", elements[0].Text);
                elements[0].Click();
                app.WaitForElementToBeVisible(By.CssSelector("nav.branches"), TimeSpan.FromSeconds(1));
                app.UrlShouldMapTo<RepositoryController>(c => c.Tree(repo, "branchX", "dir2"));

                ITH.DeleteRepositoryUsingWebsite(repo);
            });

        }

        /// <summary>
        /// Helper to run a test for every installed Git instance
        /// </summary>
        /// <param name="action"></param>
        private void ForAllGits(Action<GitInstance> action)
        {
            foreach (var git in installedgits)
            {
                Directory.CreateDirectory(WorkingDirectory);
                try
                {
                    action(git);
                }
                finally
                {
                    // Make sure we're not in the working directory when we try to delete it
                    Environment.CurrentDirectory = Path.Combine(WorkingDirectory, "..");
                    ITH.DeleteDirectory(WorkingDirectory);
                }
            }
        }

        private void PushFiles(GitInstance git, bool success)
        {
            var res = RunGitOnRepo(git, "push origin master");
            if (success)
            {
                Assert.AreEqual(string.Format(git.Resources[MsysgitResources.Definition.PushFilesSuccessError], RepositoryUrlWithoutCredentials + ".git"), res.StdErr);
            }
            else
            {
                Assert.AreEqual(string.Format(git.Resources[MsysgitResources.Definition.PushFilesFailError], BareUrl), res.StdErr);
            }
        } 

        private void SetAnonPush(bool allowAnonymousPush)
        {
            ITH.SetGlobalSetting(f => f.AllowAnonymousPush, allowAnonymousPush);
        }

        private void SetGlobalAnonPush(GitInstance git, bool allowAnonymousPush)
        {
            app.NavigateTo<SettingsController>(c => c.Index());
            var form = app.FindFormFor<GlobalSettingsModel>();
            var field =  form.Field(f => f.AllowAnonymousPush);
            ITH.SetCheckbox(field.Field, allowAnonymousPush);
            var languages = new SelectElement(form.Field(f => f.DefaultLanguage).Field);
            languages.SelectByValue("en-US");
            form.Submit();
            ITH.AssertThatNoValidationErrorOccurred();
        }

        private void CreateAndAddTestFiles(GitInstance git, int count)
        {
            foreach (var i in 0.To(count - 1))
            {
                CreateRandomFile(Path.Combine(RepositoryDirectory, "file" + i), 0);
            }
            RunGitOnRepo(git, "add .").ExpectSuccess();
            RunGitOnRepo(git, "commit -m \"Commit me! For linikfy tests: #1234\"").ExpectSuccess();
        }

        private void CloneRepoAnon(GitInstance git, bool success)
        {
            var result = RunGit(git, string.Format("clone {0}.git", RepositoryUrlWithoutCredentials), WorkingDirectory);
            if (success)
            {
                Assert.AreEqual(git.Resources[MsysgitResources.Definition.CloneEmptyRepositoryOutput], result.StdOut);
                Assert.AreEqual(git.Resources[MsysgitResources.Definition.CloneEmptyRepositoryError], result.StdErr);
            }
            else
            {
                Assert.AreEqual(git.Resources[MsysgitResources.Definition.CloneEmptyRepositoryOutput], result.StdOut);
                Assert.AreEqual(string.Format(git.Resources[MsysgitResources.Definition.CloneRepositoryFailRequiresAuthError], BareUrl), result.StdErr);
            }

        }

        private void AllowAnonRepoClone(Guid repo, bool allow)
        {
            app.NavigateTo<RepositoryController>(c => c.Edit(repo));
            var form = app.FindFormFor<RepositoryDetailModel>();
            var repo_clone = form.Field(f => f.AllowAnonymous);
            ITH.SetCheckbox(repo_clone.Field, allow);
            form.Submit();
            ITH.AssertThatNoValidationErrorOccurred();
        }

        private void CreateIdentity(GitInstance git)
        {
            RunGitOnRepo(git, "config user.name \"McFlono McFloonyloo\"").ExpectSuccess();
            RunGitOnRepo(git, "config user.email \"DontBotherMe@home.never\"").ExpectSuccess();
        }

        private void PullBranch(GitInstance git)
        {
            var result = RunGitOnRepo(git, "pull origin TestBranch");

            Assert.AreEqual("Already up-to-date.\r\n", result.StdOut);
            Assert.AreEqual(String.Format(git.Resources[MsysgitResources.Definition.PullBranchError], RepositoryUrlWithoutCredentials), result.StdErr);
        }

        private void PullTag(GitInstance git)
        {
            var result = RunGitOnRepo(git, "fetch");

            Assert.AreEqual(String.Format(git.Resources[MsysgitResources.Definition.PullTagError], RepositoryUrlWithoutCredentials), result.StdErr);
        }

        private void InitAndPullRepository(GitInstance git)
        {
            RunGitOnRepo(git, "init").ExpectSuccess();
            RunGitOnRepo(git, String.Format("remote add origin {0}", RepositoryUrlWithCredentials)).ExpectSuccess();
            var result = RunGitOnRepo(git, "pull origin master");

            Assert.AreEqual(String.Format(git.Resources[MsysgitResources.Definition.PullRepositoryError], RepositoryUrlWithoutCredentials), result.StdErr);
        }


        private void InitRepository(GitInstance git)
        {
            RunGitOnRepo(git, "init").ExpectSuccess();
            RunGitOnRepo(git, String.Format("remote add origin {0}", RepositoryUrlWithCredentials)).ExpectSuccess();
        }
        
        private void InitAndPushRepository(GitInstance git)
        {
            RunGitOnRepo(git, "init").ExpectSuccess();
            RunGitOnRepo(git, String.Format("remote add origin {0}", RepositoryUrlWithCredentials)).ExpectSuccess();
            var result = RunGitOnRepo(git, "push origin master");

            Assert.AreEqual(String.Format(git.Resources[MsysgitResources.Definition.PullRepositoryError], RepositoryUrlWithoutCredentials), result.StdErr);
        }

        private void CloneRepository(GitInstance git)
        {
            var result = RunGit(git, String.Format(String.Format("clone {0}", RepositoryUrlWithCredentials), RepositoryName), WorkingDirectory);

            Assert.AreEqual(git.Resources[MsysgitResources.Definition.CloneRepositoryOutput], result.StdOut);
            Assert.AreEqual(git.Resources[MsysgitResources.Definition.CloneRepositoryError], result.StdErr);
        }

        private void PushBranch(GitInstance git)
        {
            RunGitOnRepo(git, "checkout -b \"TestBranch\"").ExpectSuccess();
            var result = RunGitOnRepo(git, "push origin TestBranch");

            Assert.AreEqual(String.Format(git.Resources[MsysgitResources.Definition.PushBranchError], RepositoryUrlWithCredentials), result.StdErr);
        }

        private void PushTag(GitInstance git)
        {
            RunGitOnRepo(git, "tag -a v1.4 -m \"my version 1.4\"").ExpectSuccess();
            var result = RunGitOnRepo(git, "push --tags origin");
            
            Assert.AreEqual(String.Format(git.Resources[MsysgitResources.Definition.PushTagError], RepositoryUrlWithCredentials), result.StdErr);
        }

        private void CreateAndPushFiles(GitInstance git)
        {
            CreateAndAddFiles(git);
            var result = RunGitOnRepo(git, "push origin master");

            Assert.AreEqual(String.Format(git.Resources[MsysgitResources.Definition.PushFilesSuccessError], RepositoryUrlWithCredentials), result.StdErr);
        }

        private void CreateAndAddFiles(GitInstance git)
        {
            CreateRandomFile(Path.Combine(RepositoryDirectory, "1.dat"), 10);
            CreateRandomFile(Path.Combine(RepositoryDirectory, "2.dat"), 1);
            Directory.CreateDirectory(Path.Combine(RepositoryDirectory, "SubDirectory"));
            CreateRandomFile(Path.Combine(RepositoryDirectory, "Subdirectory", "3.dat"), 20);
            CreateRandomFile(Path.Combine(RepositoryDirectory, "Subdirectory", "4.dat"), 15);

            RunGitOnRepo(git, "add .").ExpectSuccess();
            RunGitOnRepo(git, "commit -m \"Test Files Added\"").ExpectSuccess();
        }

        private void CloneEmptyRepositoryWithCredentials(GitInstance git)
        {
            var result = RunGit(git, String.Format(String.Format("clone {0}", RepositoryUrlWithCredentials), RepositoryName), WorkingDirectory);
            
            Assert.AreEqual(git.Resources[MsysgitResources.Definition.CloneEmptyRepositoryOutput], result.StdOut);
            Assert.AreEqual(git.Resources[MsysgitResources.Definition.CloneEmptyRepositoryError], result.StdErr);
        }


        private GitResult RunGitOnRepo(GitInstance git, string arguments, int timeout = 30000 /* milliseconds */)
        {
            return RunGit(git, arguments, RepositoryDirectory, timeout);
        }

        private static GitResult RunGit(GitInstance git, string arguments, string workingDirectory, int timeout = 30000 /* milliseconds */)
        {
            // When a git version supports overwriting multi-value config files values this should be uncommented to make
            // all tests runnable on systems that have a credential.helper configured.
            // arguments = "-c credential.helper=\"store --file=bonobo.randomstring.credentials.txt\" " + arguments;
            Console.WriteLine("About to run '{0}' with args '{1}' in '{2}'", git, arguments, workingDirectory);
            Debug.WriteLine("About to run '{0}' with args '{1}' in '{2}'", git, arguments, workingDirectory);

            // http://stackoverflow.com/a/7608823/551045 and http://stackoverflow.com/a/22956924/551045
            using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
            using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = git.GitExe;
                    process.StartInfo.WorkingDirectory = workingDirectory;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    StringBuilder output = new StringBuilder();
                    StringBuilder error = new StringBuilder();

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            output.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(timeout) &&
                        outputWaitHandle.WaitOne(timeout) &&
                        errorWaitHandle.WaitOne(timeout))
                    {
                        var strout = output.ToString();
                        var strerr = error.ToString();

                        Console.WriteLine("Stdout: {0}", output);
                        Console.WriteLine("Stderr: {0}", error);

                        return new GitResult {StdErr = strerr, StdOut = strout, ExitCode = process.ExitCode, Resources = git.Resources};
                    }
                    else
                    {
                        Assert.Fail(string.Format("Runing command '{0} {1}' timed out! Timeout {2} seconds.", git, arguments, timeout));
                        return new GitResult() { StdErr = null, StdOut = null, ExitCode = -1 };
                    }
                }
            }
        }

        private void CreateRandomFile(string fileName, int sizeInMb)
        {
            const int blockSize = 1024 * 8;
            const int blocksPerMb = (1024 * 1024) / blockSize;
            byte[] data = new byte[blockSize];
            Random rng = new Random();
            using (FileStream stream = File.OpenWrite(fileName))
            {
                for (int i = 0; i < sizeInMb * blocksPerMb; i++)
                {
                    rng.NextBytes(data);
                    stream.Write(data, 0, data.Length);
                }
            }
        }

    }
}
