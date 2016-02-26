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
using System.Text;
using System.Threading;

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
                    var resource = new MsysgitResources(version);
                    installedgits.Add(Tuple.Create(git, resource));
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
            EnsureCredentialHelperStore(installedgits.Last());
        }

        private void EnsureCredentialHelperStore(Tuple<string, MsysgitResources> tuple)
        {
            Directory.CreateDirectory(WorkingDirectory);
            var git = tuple.Item1;
            var res = RunGit(git, "config --get credential.helper", WorkingDirectory);

            Assert.AreEqual("store --file=bonobo.randomstring.credentials.txt\r\n", res.Item1);
            DeleteDirectory(WorkingDirectory);
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void RunGitTests()
        {

            foreach (var gitresource in installedgits)
            {

                Directory.CreateDirectory(WorkingDirectory);
                var git = gitresource.Item1;
                var resource = gitresource.Item2;

                try
                {
                    Guid repo_id = CreateRepositoryOnWebInterface();
                    CloneEmptyRepositoryAndEnterRepo(git, resource);
                    CreateIdentity(git);
                    CreateAndPushFiles(git, resource);
                    PushTag(git, resource);
                    PushBranch(git, resource);

                    DeleteDirectory(RepositoryDirectory);
                    CloneRepository(git, resource);

                    DeleteDirectory(RepositoryDirectory);
                    Directory.CreateDirectory(RepositoryDirectory);
                    InitAndPullRepository(git, resource);
                    PullTag(git, resource);
                    PullBranch(git, resource);
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
                Directory.CreateDirectory(WorkingDirectory);
                var git = gitres.Item1;
                var resource = gitres.Item2;
                try
                {
                    Guid repo_id = CreateRepositoryOnWebInterface();
                    RemoveStoredCredentials(git);
                    AllowAnonRepoClone(repo_id, false);
                    CloneRepoAnon(git, resource, false);
                    AllowAnonRepoClone(repo_id, true);
                    CloneRepoAnon(git, resource, true);
                    DeleteRepository(repo_id);
                }
                finally
                {
                    DeleteDirectory(WorkingDirectory);
                }
            }
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void NoDeadlockLargeOutput()
        {
            var gitres = installedgits.Last();
            var git = gitres.Item1;
            var resource = gitres.Item2;
            Directory.CreateDirectory(WorkingDirectory);

            try{
                var repo_id = CreateRepositoryOnWebInterface();
                CloneEmptyRepositoryAndEnterRepo(git, resource);
                CreateIdentity(git);
                CreateAndAddTestFiles(git, 2000);
                DeleteRepository(repo_id);
            }
            finally
            {
                DeleteDirectory(WorkingDirectory);
            }
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void AnonPush()
        {

            var gitres = installedgits.Last();
            var git = gitres.Item1;
            var resource = gitres.Item2;
            Directory.CreateDirectory(WorkingDirectory);

            try
            {
                var repo_id = CreateRepositoryOnWebInterface();
                RemoveStoredCredentials(git);
                AllowAnonRepoClone(repo_id, true);
                CloneRepoAnon(git, resource, true);
                CreateIdentity(git);
                SetAnonPush(git, false);
                CreateRandomFile(Path.Combine(RepositoryDirectory, "file.txt"), 0);
                RunGitOnRepo(git, "add .");
                RunGitOnRepo(git, "commit -m\"Aw yeah!\"");
                PushFiles(git, resource, false);
                SetAnonPush(git, true);
                PushFiles(git, resource, true);
                DeleteRepository(repo_id);
            }
            finally
            {
                DeleteDirectory(WorkingDirectory);
            }

        }

        private void PushFiles(string git, MsysgitResources resource, bool success)
        {
            var res = RunGitOnRepo(git, "push origin master");
            if (success)
            {
                Assert.AreEqual(string.Format(resource[MsysgitResources.Definition.PushFilesSuccessError], RepositoryUrlWithoutCredentials + ".git"), res.Item2);
            }
            else
            {
                Assert.AreEqual(string.Format(resource[MsysgitResources.Definition.PushFilesFailError], RepositoryUrlWithoutCredentials), res.Item2);
            }
        } 

        private void RemoveCredentialsFromCloneUrl(string git)
        {
            RunGitOnRepo(git, "remote set-url origin " + Url + RepositoryName + ".git");
        }

        private void SetCheckbox<T>(FluentField<T, bool> field, bool select) where T : class
        {
            if (select)
            {
                if (!field.Field.Selected)
                {
                    field.Click();
                }
            }
            else
            {
                if(field.Field.Selected)
                {
                    field.Click();
                }
            }
        }

        private void SetAnonPush(string git, bool allowAnonymousPush)
        {
            app.NavigateTo<SettingsController>(c => c.Index());
            var form = app.FindFormFor<GlobalSettingsModel>();
            var field =  form.Field(f => f.AllowAnonymousPush);
            SetCheckbox(field, allowAnonymousPush);
            form.Submit();
        }

        private void CreateAndAddTestFiles(string git, int count)
        {
            foreach (var i in 0.To(count - 1))
            {
                CreateRandomFile(Path.Combine(RepositoryDirectory, "file" + i.ToString()), 0);
            }
            RunGitOnRepo(git, "add .");
            RunGitOnRepo(git, "commit -m \"Commit me!\"");
        }

        private void RemoveStoredCredentials(string git)
        {
            File.Delete("Bonobo.randomstring.credentials.txt");
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
            SetCheckbox(repo_clone, allow);
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

        private Guid CreateRepositoryOnWebInterface()
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

        private void PullBranch(string git, MsysgitResources resource)
        {
            var result = RunGitOnRepo(git, "pull origin TestBranch");

            Assert.AreEqual("Already up-to-date.\r\n", result.Item1);
            Assert.AreEqual(String.Format(resource[MsysgitResources.Definition.PullBranchError], RepositoryUrlWithoutCredentials), result.Item2);
        }

        private void PullTag(string git, MsysgitResources resource)
        {
            var result = RunGitOnRepo(git, "fetch");

            Assert.AreEqual(String.Format(resource[MsysgitResources.Definition.PullTagError], RepositoryUrlWithoutCredentials), result.Item2);
        }

        private void InitAndPullRepository(string git, MsysgitResources resource)
        {

            RunGitOnRepo(git, "init");
            RunGitOnRepo(git, String.Format("remote add origin {0}", RepositoryUrlWithCredentials));
            var result = RunGitOnRepo(git, "pull origin master");

            Assert.AreEqual(String.Format(resource[MsysgitResources.Definition.PullRepositoryError], RepositoryUrlWithoutCredentials), result.Item2);
        }

        private void CloneRepository(string git, MsysgitResources resource)
        {
            var result = RunGit(git, String.Format(String.Format("clone {0}", RepositoryUrlWithCredentials), RepositoryName), WorkingDirectory);

            Assert.AreEqual(resource[MsysgitResources.Definition.CloneRepositoryOutput], result.Item1);
            Assert.AreEqual(resource[MsysgitResources.Definition.CloneRepositoryError], result.Item2);
        }

        private void PushBranch(string git, MsysgitResources resource)
        {
            RunGitOnRepo(git, "checkout -b \"TestBranch\"");
            var result = RunGitOnRepo(git, "push origin TestBranch");

            Assert.AreEqual(String.Format(resource[MsysgitResources.Definition.PushBranchError], RepositoryUrlWithCredentials), result.Item2);
        }

        private void PushTag(string git, MsysgitResources resource)
        {
            RunGitOnRepo(git, "tag -a v1.4 -m \"my version 1.4\"");
            var result = RunGitOnRepo(git, "push --tags origin");
            
            Assert.AreEqual(String.Format(resource[MsysgitResources.Definition.PushTagError], RepositoryUrlWithCredentials), result.Item2);
        }

        private void CreateAndPushFiles(string git, MsysgitResources resource)
        {
            CreateRandomFile(Path.Combine(RepositoryDirectory, "1.dat"), 10);
            CreateRandomFile(Path.Combine(RepositoryDirectory, "2.dat"), 1);
            Directory.CreateDirectory(Path.Combine(RepositoryDirectory, "SubDirectory"));
            CreateRandomFile(Path.Combine(RepositoryDirectory, "Subdirectory", "3.dat"), 20);
            CreateRandomFile(Path.Combine(RepositoryDirectory, "Subdirectory", "4.dat"), 15);

            RunGitOnRepo(git, "add .");
            RunGitOnRepo(git, "commit -m \"Test Files Added\"");
            var result = RunGitOnRepo(git, "push origin master");

            Assert.AreEqual(String.Format(resource[MsysgitResources.Definition.PushFilesSuccessError], RepositoryUrlWithCredentials), result.Item2);
        }

        private void CloneEmptyRepositoryAndEnterRepo(string git, MsysgitResources resource)
        {
            var result = RunGit(git, String.Format(String.Format("clone {0}", RepositoryUrlWithCredentials), RepositoryName), WorkingDirectory);
            
            Assert.AreEqual(resource[MsysgitResources.Definition.CloneEmptyRepositoryOutput], result.Item1);
            Assert.AreEqual(resource[MsysgitResources.Definition.CloneEmptyRepositoryError], result.Item2);
        }


        private Tuple<string, string> RunGitOnRepo(string git, string arguments, int timeout = 30000 /* milliseconds */)
        {
            return RunGit(git, arguments, RepositoryDirectory, timeout);
        }

        private Tuple<string, string> RunGit(string git, string arguments, string workingDirectory, int timeout = 30000 /* milliseconds */)
        {
            arguments = "-c credential.helper=\"store --file=bonobo.randomstring.credentials.txt\" " + arguments;
            Console.WriteLine("About to run '{0}' with args '{1}' in '{2}'", git, arguments, workingDirectory);
            Debug.WriteLine("About to run '{0}' with args '{1}' in '{2}'", git, arguments, workingDirectory);

            // http://stackoverflow.com/a/7608823/551045 and http://stackoverflow.com/a/22956924/551045
            using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
            using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = git;
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

                        Console.WriteLine("Output: {0}", output);
                        Console.WriteLine("Errors: {0}", error);

                        return Tuple.Create(strout, strerr);
                    }
                    else
                    {
                        Assert.Fail(string.Format("Runing command '{0} {1}' timed out! Timeout {2} seconds.", git, arguments, timeout));
                        return Tuple.Create<string, string>(null, null);
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

            var directory = new DirectoryInfo(directoryPath) { Attributes = FileAttributes.Normal };
            foreach (var item in directory.GetFiles("*.*", SearchOption.AllDirectories))
            {
                item.Attributes = FileAttributes.Normal;
            }
            directory.Delete(true);
        }

    }
}
