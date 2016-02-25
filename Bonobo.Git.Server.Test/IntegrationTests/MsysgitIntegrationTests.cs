using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using SpecsFor.Mvc;
using OpenQA.Selenium;

using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Test.Integration.ClAndWeb
{
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
        private readonly static string RepositoryUrl = "http://{0}localhost:20000/{2}{1}";
        private readonly static string RepositoryUrlWithoutCredentials = String.Format(RepositoryUrl, String.Empty, String.Empty, RepositoryName);
        private readonly static string RepositoryUrlWithCredentials = String.Format(RepositoryUrl, Credentials, ".git", RepositoryName);
        private readonly static string Url = string.Format(RepositoryUrl, string.Empty, string.Empty, string.Empty);

        List<Tuple<string, MsysgitResources>> installedgits = new List<Tuple<string, MsysgitResources>>();

        private static MvcWebApp app;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            // Make sure relative paths are frozen in case the app's CurrentDir changes
            WorkingDirectory = Path.GetFullPath(WorkingDirectory);
            GitPath = Path.GetFullPath(GitPath);
            RepositoryDirectory = Path.Combine(WorkingDirectory, RepositoryName);
            
            app = new MvcWebApp();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            app.Browser.Close();
            app.Browser.Dispose();
        }

        [TestInitialize]
        public void Initialize()
        {
            DeleteDirectory(WorkingDirectory);
            bool any_git_installed = false;
            List<string> not_found = new List<string>();

            foreach (var version in GitVersions)
            {
                var git = String.Format(GitPath, version);
                if (File.Exists(git))
                {
                    var resources = new MsysgitResources(version);
                    installedgits.Add(Tuple.Create(git, resources));
                    any_git_installed = true;
                }
                else
                {
                    not_found.Add(git);
                }
            }

            if (!any_git_installed)
            {
                Assert.Fail(string.Format("Please ensure that you have at least one git installation in '{0}'.", string.Join("', '", not_found.Select(n => Path.GetFullPath(n)))));
            }
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void RunGitTests()
        {
            foreach (var gitresource in installedgits)
            {

                Directory.CreateDirectory(WorkingDirectory);
                var git = gitresource.Item1;
                var resources = gitresource.Item2;

                try
                {
                    Guid repo_id = CreateRepository();
                    CloneEmptyRepositoryAndEnterRepo(git, resources);
                    CreateIdentity(git);
                    PushFiles(git, resources);
                    PushTag(git, resources);
                    PushBranch(git, resources);
                    CloneRepository(git, resources);
                    PullRepository(git, resources);
                    PullTag(git, resources);
                    PullBranch(git, resources);
                    DeleteRepository(repo_id);
                }
                finally
                {
                    DeleteDirectory(WorkingDirectory);
                }
            }
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void AnonRepoClone()
        {
            foreach (var gitres in installedgits)
            {
                string old_helper = null;
                Directory.CreateDirectory(WorkingDirectory);
                var git = gitres.Item1;
                var resource = gitres.Item2;
                try
                {
                    Guid repo = CreateRepository();
                    old_helper = DisableCredentialHelper(git);
                    AllowAnonRepoClone(repo, false);
                    CloneRepoAnon(git, resource, false);
                    AllowAnonRepoClone(repo, true);
                    CloneRepoAnon(git, resource, true);
                }
                finally
                {
                    RestoreCredentialHelper(git, old_helper);
                    DeleteDirectory(WorkingDirectory);
                }
            }
        }

        private void RestoreCredentialHelper(string git, string old_helper)
        {
            if (old_helper != null)
            {
                var helper_location = string.Format("credential.{0}.helper", Url);
                RunGit(git, "config " + helper_location + " " + old_helper, WorkingDirectory);
            }
        }

        private string DisableCredentialHelper(string git)
        {
            var helper_location = string.Format("credential.{0}.helper", Url);
            var result = RunGit(git, "config --get " + helper_location, WorkingDirectory);
            RunGit(git, "config " + helper_location + " \"\"", WorkingDirectory);
            var current_helper = result.Item1;
            var last_newline = current_helper.LastIndexOf('\n');
            if (last_newline != -1)
            {
                current_helper = current_helper.Substring(0, last_newline);
            }
            return current_helper;
        }

        private void CloneRepoAnon(string git, MsysgitResources resource, bool success)
        {
            var result = RunGit(git, string.Format("clone {0}.git", RepositoryUrlWithoutCredentials), WorkingDirectory);
            if (success)
            {
                Assert.AreEqual(resource[MsysgitResources.Definition.CloneEmptyRepositoryOutput], result.Item1);
                Assert.AreEqual(resource[MsysgitResources.Definition.CloneEmptyRepositoryError], result.Item2);
            }
            else
            {
                Assert.AreEqual(resource[MsysgitResources.Definition.CloneEmptyRepositoryOutput], result.Item1);
                Assert.AreEqual(string.Format(resource[MsysgitResources.Definition.CloneRepositoryFailRequiresAuthError], Url.Substring(0, Url.Length - 1)), result.Item2);
            }

        }

        private void AllowAnonRepoClone(Guid repo, bool allow)
        {
            app.NavigateTo<RepositoryController>(c => c.Edit(repo));
            var form = app.FindFormFor<RepositoryDetailModel>();
            var repo_clone = form.Field(f => f.AllowAnonymous);
            if (allow)
            {
                if (!repo_clone.Field.Selected)
                {
                    repo_clone.Field.Click();
                }
            }
            else
            {
                if (repo_clone.Field.Selected)
                {
                    repo_clone.Field.Click();
                }
            }
            form.Submit();
        }

        private void CreateIdentity(string git)
        {
            RunGitOnRepo(git, "config user.name \"McFlono McFloonyloo\"");
            RunGitOnRepo(git, "config user.email \"DontBotherMe@home.never\"");
        }

        private void DeleteRepository(Guid guid)
        {
            app.NavigateTo<RepositoryController>(c => c.Delete(guid));
            app.FindFormFor<RepositoryDetailModel>().Submit();

            // make sure it no longer is listed
            app.NavigateTo<RepositoryController>(c => c.Index(null, null));
            try
            {
                var ele = app.Browser.FindElement(By.Id("Repositories"));
                Assert.Fail("Table should not exist without repositories!");
            }
            catch (NoSuchElementException exc)
            {
                if (!exc.Message.Contains(" == Repositories"))
                {
                    throw;
                }
            }
        }

        private Guid CreateRepository()
        {
            app.NavigateTo<RepositoryController>(c => c.Create());
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo("Integration")
                .Submit();

            // ensure it appears on the listing
            app.NavigateTo<RepositoryController>(c => c.Index(null, null));

            var rpm = app.FindDisplayFor<IEnumerable<RepositoryDetailModel>>();
            //var l_to = app.FindLinkTo<RepositoryController>(c => c.Detail(Guid.NewGuid()));
            var repo_item = rpm.DisplayFor(s => s.First().Name);
            Assert.AreEqual(repo_item.Text, "Integration");
            return new Guid(repo_item.GetAttribute("data-repo-id"));
        }

        private void PullBranch(string git, MsysgitResources resources)
        {
            var result = RunGitOnRepo(git, "pull origin TestBranch");

            Assert.AreEqual("Already up-to-date.\n", result.Item1);
            Assert.AreEqual(String.Format(resources[MsysgitResources.Definition.PullBranchError], RepositoryUrlWithoutCredentials), result.Item2);
        }

        private void PullTag(string git, MsysgitResources resources)
        {
            var result = RunGitOnRepo(git, "fetch");

            Assert.AreEqual(String.Format(resources[MsysgitResources.Definition.PullTagError], RepositoryUrlWithoutCredentials), result.Item2);
        }

        private void PullRepository(string git, MsysgitResources resources)
        {
            DeleteDirectory(RepositoryDirectory);
            Directory.CreateDirectory(RepositoryDirectory);

            RunGitOnRepo(git, "init");
            RunGitOnRepo(git, String.Format("remote add origin {0}", RepositoryUrlWithCredentials));
            var result = RunGitOnRepo(git, "pull origin master");

            Assert.AreEqual(String.Format(resources[MsysgitResources.Definition.PullRepositoryError], RepositoryUrlWithoutCredentials), result.Item2);
        }

        private void CloneRepository(string git, MsysgitResources resources)
        {
            DeleteDirectory(RepositoryDirectory);
            var result = RunGit(git, String.Format(String.Format("clone {0}", RepositoryUrlWithCredentials), RepositoryName), WorkingDirectory);

            Assert.AreEqual(resources[MsysgitResources.Definition.CloneRepositoryOutput], result.Item1);
            Assert.AreEqual(resources[MsysgitResources.Definition.CloneRepositoryError], result.Item2);
        }

        private void PushBranch(string git, MsysgitResources resources)
        {
            RunGitOnRepo(git, "checkout -b \"TestBranch\"");
            var result = RunGitOnRepo(git, "push origin TestBranch");

            Assert.AreEqual(String.Format(resources[MsysgitResources.Definition.PushBranchError], RepositoryUrlWithCredentials), result.Item2);
        }

        private void PushTag(string git, MsysgitResources resources)
        {
            RunGitOnRepo(git, "tag -a v1.4 -m \"my version 1.4\"");
            var result = RunGitOnRepo(git, "push --tags origin");
            
            Assert.AreEqual(String.Format(resources[MsysgitResources.Definition.PushTagError], RepositoryUrlWithCredentials), result.Item2);
        }

        private void PushFiles(string git, MsysgitResources resources)
        {
            CreateRandomFile(Path.Combine(RepositoryDirectory, "1.dat"), 10);
            CreateRandomFile(Path.Combine(RepositoryDirectory, "2.dat"), 1);
            Directory.CreateDirectory(Path.Combine(RepositoryDirectory, "SubDirectory"));
            CreateRandomFile(Path.Combine(RepositoryDirectory, "Subdirectory", "3.dat"), 20);
            CreateRandomFile(Path.Combine(RepositoryDirectory, "Subdirectory", "4.dat"), 15);

            RunGitOnRepo(git, "add .");
            RunGitOnRepo(git, "commit -m \"Test Files Added\"");
            var result = RunGitOnRepo(git, "push origin master");

            Assert.AreEqual(String.Format(resources[MsysgitResources.Definition.PushFilesError], RepositoryUrlWithCredentials), result.Item2);
        }

        private void CloneEmptyRepositoryAndEnterRepo(string git, MsysgitResources resources)
        {
            var result = RunGit(git, String.Format(String.Format("clone {0}", RepositoryUrlWithCredentials), RepositoryName), WorkingDirectory);
            
            Assert.AreEqual(resources[MsysgitResources.Definition.CloneEmptyRepositoryOutput], result.Item1);
            Assert.AreEqual(resources[MsysgitResources.Definition.CloneEmptyRepositoryError], result.Item2);
        }


        private Tuple<string, string> RunGitOnRepo(string git, string arguments)
        {
            return RunGit(git, arguments, RepositoryDirectory);
        }

        private Tuple<string, string> RunGit(string git, string arguments, string workingDirectory)
        {
            Console.WriteLine("About to run '{0}' with args '{1}' in '{2}'", git, arguments, workingDirectory);
            Debug.WriteLine("About to run '{0}' with args '{1}' in '{2}'", git, arguments, workingDirectory);

            using (var process = new Process())
            {
                process.StartInfo.FileName = git;
                process.StartInfo.WorkingDirectory = workingDirectory;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                process.WaitForExit();

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                Console.WriteLine("Output: {0}", output);
                Console.WriteLine("Errors: {0}", error);

                return new Tuple<string, string>(output, error);
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

            var directory = new DirectoryInfo(directoryPath) { Attributes = FileAttributes.Normal };
            foreach (var item in directory.GetFiles("*.*", SearchOption.AllDirectories))
            {
                item.Attributes = FileAttributes.Normal;
            }
            directory.Delete(true);
        }

    }
}
