using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Test.IntegrationTests;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Support.UI;
using SpecsFor.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

namespace Bonobo.Git.Server.Test.Integration.ClAndWeb
{
    using ITH = IntegrationTestHelpers;

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
        private const string RepositoryName = "Integration";
        private static string WorkingDirectory = @"..\..\..\Tests\IntegrationTests";
        private static string GitPath = @"..\..\..\Gits\{0}\bin\git.exe";
        private static string RepositoryDirectory = Path.Combine(WorkingDirectory, RepositoryName);
        private readonly static string ServerRepositoryPath = Path.Combine(@"..\..\..\Bonobo.Git.Server\App_Data\Repositories", RepositoryName);
        private readonly static string ServerRepositoryBackupPath = Path.Combine(@"..\..\..\Tests\", RepositoryName, "Backup");
        private readonly static string[] GitVersions = { "1.7.4", "1.7.6", "1.7.7.1", "1.7.8", "1.7.9", "1.8.0", "1.8.1.2", "1.8.3", "1.9.5", "2.6.1" };
        private readonly static string Credentials = "admin:admin@";
        private readonly static string RepositoryUrlTemplate = "http://{0}localhost:20000/{2}{1}";
        private readonly static string RepositoryUrlWithoutCredentials = String.Format(RepositoryUrlTemplate, String.Empty, String.Empty, RepositoryName);
        private readonly static string RepositoryUrlWithCredentials = String.Format(RepositoryUrlTemplate, Credentials, ".git", RepositoryName);
        private readonly static string Url = string.Format(RepositoryUrlTemplate, string.Empty, string.Empty, string.Empty);
        private readonly static string BareUrl = Url.TrimEnd('/');

        private static List<GitInstance> installedgits = new List<GitInstance>();

        private static MvcWebApp app;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            // Make sure relative paths are frozen in case the app's CurrentDir changes
            WorkingDirectory = Path.GetFullPath(WorkingDirectory);
            GitPath = Path.GetFullPath(GitPath);
            RepositoryDirectory = Path.Combine(WorkingDirectory, RepositoryName);
            
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
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            app.Browser.Close();
        }

        [TestInitialize]
        public void Initialize()
        {
            DeleteDirectory(WorkingDirectory);
            IntegrationTestHelpers.Login(app);
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void RunGitTests()
        {

            ForAllGits(git =>
                {
                    using (var repo_id = ITH.CreateRepositoryOnWebInterface(app, RepositoryName))
                    {
                        CloneEmptyRepositoryWithCredentials(git);
                        CreateIdentity(git);
                        CreateAndPushFiles(git);
                        PushTag(git);
                        PushBranch(git);

                        DeleteDirectory(RepositoryDirectory);
                        CloneRepository(git);

                        DeleteDirectory(RepositoryDirectory);
                        Directory.CreateDirectory(RepositoryDirectory);
                        InitAndPullRepository(git);
                        PullTag(git);
                        PullBranch(git);
                    }
                });
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
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
                using (var repo_id = ITH.CreateRepositoryOnWebInterface(app, RepositoryName))
                {
                    AllowAnonRepoClone(repo_id, false);
                    CloneRepoAnon(git, false);
                    AllowAnonRepoClone(repo_id, true);
                    CloneRepoAnon(git, true);
                }
            });
        }

        private static bool AnyCredentialHelperExists(GitInstance git)
        {
            IEnumerable<string> urls = new List<string>
            {
                string.Format(RepositoryUrlTemplate, string.Empty, string.Empty, string.Empty),
                string.Format(RepositoryUrlTemplate, Credentials, string.Empty, string.Empty),
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

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void NoDeadlockOnLargeOutput()
        {
            var git = installedgits.Last();
            Directory.CreateDirectory(WorkingDirectory);

            try{
                using (var repo_id = ITH.CreateRepositoryOnWebInterface(app, RepositoryName))
                {
                    CloneEmptyRepositoryWithCredentials(git);
                    CreateIdentity(git);
                    CreateAndAddTestFiles(git, 2000);
                }
            }
            finally
            {
                DeleteDirectory(WorkingDirectory);
            }
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void RepoAnonPushRespectsGlobalSettings()
        {

            ForAllGits(git =>
            {
                using (var repo_id = ITH.CreateRepositoryOnWebInterface(app, RepositoryName))
                {
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

                }
            });
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void RepoAnonPushYesOverridesGlobalSettings()
        {

            ForAllGits(git =>
            {
                using (var repo_id = ITH.CreateRepositoryOnWebInterface(app, RepositoryName))
                {
                    AllowAnonRepoClone(repo_id, true);
                    CloneRepoAnon(git, true);
                    CreateIdentity(git);
                    SetRepoPushTo(repo_id, RepositoryPushMode.Yes);

                    CreateRandomFile(Path.Combine(RepositoryDirectory, "file.txt"), 0);
                    RunGitOnRepo(git, "add .");
                    RunGitOnRepo(git, "commit -m\"Aw yeah!\"");

                    SetGlobalAnonPush(git, false);
                    PushFiles(git, true);

                }
            });
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void RepoAnonPushNoOverridesGlobalSettings()
        {

            ForAllGits(git =>
            {
                using (var repo_id = ITH.CreateRepositoryOnWebInterface(app, RepositoryName))
                {
                    AllowAnonRepoClone(repo_id, true);
                    CloneRepoAnon(git, true);
                    CreateIdentity(git);
                    SetRepoPushTo(repo_id, RepositoryPushMode.No);

                    CreateRandomFile(Path.Combine(RepositoryDirectory, "file.txt"), 0);
                    RunGitOnRepo(git, "add .");
                    RunGitOnRepo(git, "commit -m\"Aw yeah!\"");

                    SetGlobalAnonPush(git, true);
                    PushFiles(git, false);

                }
            });
        }

        private void SetRepoPushTo(Guid repo_id, RepositoryPushMode repositoryPushStatus)
        {
            app.NavigateTo<RepositoryController>(c => c.Edit(repo_id));
            var form = app.FindFormFor<RepositoryDetailModel>();
            var select = new SelectElement(form.Field(f => f.AllowAnonymousPush).Field);
            select.SelectByValue(repositoryPushStatus.ToString("D"));
            form.Submit();
        }

        /// <summary>
        /// This does an authorized push to a repo that allows anon clone
        /// At the time of writing, this demonstrates an issue which breaks authorised push if the repo allows anon pull
        /// </summary>
        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void NamedPushToAnonRepo()
        {
            ForAllGits(git =>
            {
                using (var repo_id = ITH.CreateRepositoryOnWebInterface(app, RepositoryName))
                {

                    // Clone the repo
                    AllowAnonRepoClone(repo_id, true);
                    CloneEmptyRepositoryWithCredentials(git);

                    CreateIdentity(git);
                    // I want to do a push *with* a username
                    CreateAndPushFiles(git);

                }
            });
        }
        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
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

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void PushToCreateIsAllowedIfOptionIsSet()
        {
            ForAllGits(git =>
            {
                // Create a repo locally
                Directory.CreateDirectory(RepositoryDirectory);
                InitRepository(git);
                Environment.CurrentDirectory = RepositoryDirectory;
                CreateIdentity(git);
                CreateAndAddFiles(git);

                // Enable the push-to-create option
                SetGlobalSetting(x => x.AllowPushToCreate, true);

                RunGitOnRepo(git, "push origin master").ExpectSuccess();

                // Ensure repo is created with same name as was pushed
                Guid repoId = ITH.FindRepository(app, RepositoryName);
                Assert.AreNotEqual(Guid.Empty, repoId);

                ITH.DeleteRepositoryUsingWebsite(app, repoId);
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
                    DeleteDirectory(WorkingDirectory);
                }
            }
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void NavigateReposWithDropdown()
        {
            
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
            SetGlobalSetting(f => f.AllowAnonymousPush, allowAnonymousPush);
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
        }

        private void CreateAndAddTestFiles(GitInstance git, int count)
        {
            foreach (var i in 0.To(count - 1))
            {
                CreateRandomFile(Path.Combine(RepositoryDirectory, "file" + i), 0);
            }
            RunGitOnRepo(git, "add .").ExpectSuccess();
            RunGitOnRepo(git, "commit -m \"Commit me!\"").ExpectSuccess();
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
        }

        private void SetGlobalSetting(Expression<Func<GlobalSettingsModel, bool>> optionExpression, bool value)
        {
            app.NavigateTo<SettingsController>(c => c.Index());
            var form = app.FindFormFor<GlobalSettingsModel>();
            ITH.SetCheckbox(form.Field(optionExpression).Field, value);
            form.Submit();
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

        private void DeleteDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return;

            // We have to tolerate intermittent errors during directory deletion, because
            // other parts of Windows sometimes hold locks on files briefly
            // Multiple tries normally fixes it
            for (int attempt = 10; attempt >= 0; attempt--)
            {
                try
                {
                    var directory = new DirectoryInfo(directoryPath) {Attributes = FileAttributes.Normal};
                    foreach (var item in directory.GetFiles("*.*", SearchOption.AllDirectories))
                    {
                        item.Attributes = FileAttributes.Normal;
                    }
                    directory.Delete(true);
                    return;
                }
                catch
                {
                    if (attempt == 0)
                    {
                        throw;
                    }
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
