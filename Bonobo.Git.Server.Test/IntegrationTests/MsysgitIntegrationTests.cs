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
        private const string WorkingDirectory = @"..\..\..\Tests\IntegrationTests";
        private const string GitPath = @"..\..\..\Gits\{0}\bin\git.exe";
        private readonly static string RepositoryDirectory = Path.Combine(WorkingDirectory, RepositoryName);
        private readonly static string ServerRepositoryPath = Path.Combine(@"..\..\..\Bonobo.Git.Server\App_Data\Repositories", RepositoryName);
        private readonly static string ServerRepositoryBackupPath = Path.Combine(@"..\..\..\Tests\", RepositoryName, "Backup");
        private readonly static string[] GitVersions = { "1.7.4", "1.7.6", "1.7.7.1", "1.7.8", "1.7.9", "1.8.0", "1.8.1.2", "1.8.3", "1.9.5", "2.6.1" };
        private readonly static string Credentials = "admin:admin@";
        private readonly static string RepositoryUrl = "http://{0}localhost:20000/{2}{1}";
        private readonly static string RepositoryUrlWithoutCredentials = String.Format(RepositoryUrl, String.Empty, String.Empty, RepositoryName);
        private readonly static string RepositoryUrlWithCredentials = String.Format(RepositoryUrl, Credentials, ".git", RepositoryName);

        private readonly static Dictionary<string, Dictionary<string, string>> Resources = new Dictionary<string, Dictionary<string, string>>
        {
            { String.Format(GitPath,  "1.7.4"), new Dictionary<string, string> 
            {
                { "Key", "Value" }
            }}
        };

        private static MvcWebApp app;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            app = new MvcWebApp();
        }


        [TestInitialize]
        public void Initialize()
        {
            DeleteDirectory(WorkingDirectory);
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void RunGitTests()
        {
            bool has_any_test_run = false;
            List<string> gitpaths = new List<string>();
            foreach (var version in GitVersions)
            {
                var git = String.Format(GitPath, version);
                if (!File.Exists(git))
                {
                    gitpaths.Add(git);
                    continue;
                }
                var resources = new MsysgitResources(version);

                Directory.CreateDirectory(WorkingDirectory);

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
                has_any_test_run = true;
            }

            if (!has_any_test_run)
            {
                Assert.Fail(string.Format("Please ensure that you have at least one git installation in '{0}'.", string.Join("', '", gitpaths.Select(n => Path.GetFullPath(n)))));
            }

        }

        private void CreateIdentity(string git)
        {
            RunGit(git, "config user.name \"McFlono McFloonyloo\"");
            RunGit(git, "config user.email \"DontBotherMe@home.never\"");
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
            var result = RunGit(git, "pull origin TestBranch");

            Assert.AreEqual("Already up-to-date.\n", result.Item1);
            Assert.AreEqual(String.Format(resources[MsysgitResources.Definition.PullBranchError], RepositoryUrlWithoutCredentials), result.Item2);
        }

        private void PullTag(string git, MsysgitResources resources)
        {
            var result = RunGit(git, "fetch");

            Assert.AreEqual(String.Format(resources[MsysgitResources.Definition.PullTagError], RepositoryUrlWithoutCredentials), result.Item2);
        }

        private void PullRepository(string git, MsysgitResources resources)
        {
            DeleteDirectory(RepositoryDirectory);
            Directory.CreateDirectory(RepositoryDirectory);

            RunGit(git, "init");
            RunGit(git, String.Format("remote add origin {0}", RepositoryUrlWithCredentials));
            var result = RunGit(git, "pull origin master");

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
            RunGit(git, "checkout -b \"TestBranch\"");
            var result = RunGit(git, "push origin TestBranch");

            Assert.AreEqual(String.Format(resources[MsysgitResources.Definition.PushBranchError], RepositoryUrlWithCredentials), result.Item2);
        }

        private void PushTag(string git, MsysgitResources resources)
        {
            RunGit(git, "tag -a v1.4 -m \"my version 1.4\"");
            var result = RunGit(git, "push --tags origin");
            
            Assert.AreEqual(String.Format(resources[MsysgitResources.Definition.PushTagError], RepositoryUrlWithCredentials), result.Item2);
        }

        private void PushFiles(string git, MsysgitResources resources)
        {
            CreateRandomFile(Path.Combine(RepositoryDirectory, "1.dat"), 10);
            CreateRandomFile(Path.Combine(RepositoryDirectory, "2.dat"), 1);
            Directory.CreateDirectory(Path.Combine(RepositoryDirectory, "SubDirectory"));
            CreateRandomFile(Path.Combine(RepositoryDirectory, "Subdirectory", "3.dat"), 20);
            CreateRandomFile(Path.Combine(RepositoryDirectory, "Subdirectory", "4.dat"), 15);

            RunGit(git, "add .");
            RunGit(git, "commit -m \"Test Files Added\"");
            var result = RunGit(git, "push origin master");

            Assert.AreEqual(String.Format(resources[MsysgitResources.Definition.PushFilesError], RepositoryUrlWithCredentials), result.Item2);
        }

        private void CloneEmptyRepositoryAndEnterRepo(string git, MsysgitResources resources)
        {
            var result = RunGit(git, String.Format(String.Format("clone {0}", RepositoryUrlWithCredentials), RepositoryName), WorkingDirectory);
            
            Assert.AreEqual(resources[MsysgitResources.Definition.CloneEmptyRepositoryOutput], result.Item1);
            Assert.AreEqual(resources[MsysgitResources.Definition.CloneEmptyRepositoryError], result.Item2);
        }


        private void RestoreServerRepository()
        {
            CopyOverrideDirectory(ServerRepositoryBackupPath, ServerRepositoryPath);
        }

        private void BackupServerRepository()
        {
            CopyOverrideDirectory(ServerRepositoryPath, ServerRepositoryBackupPath);
        }

        private void CopyOverrideDirectory(string target, string destination)
        {
            DeleteDirectory(destination);
            Directory.CreateDirectory(destination);


            foreach (string dirPath in Directory.GetDirectories(target, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(target, destination));
            }

            foreach (string newPath in Directory.GetFiles(target, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(target, destination));
            }
        }

        private Tuple<string, string> RunGit(string git, string arguments)
        {
            return RunGit(git, arguments, RepositoryDirectory);
        }

        private Tuple<string, string> RunGit(string git, string arguments, string workingDirectory)
        {
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
