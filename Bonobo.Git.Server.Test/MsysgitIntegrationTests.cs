using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace Bonobo.Git.Server.Test
{
    /// <summary>
    /// This is a regression test for msysgit clients. It can be run against installed version of Bonobo Git Server.
    /// It requires empty Integration repository created on the server before first run. It backups and restores the data when the test is finished. Therefore can be run multiple times.
    /// </summary>
    [TestClass]
    public class MsysgitIntegrationTests
    {
        private const string RepositoryName = "Integration";
        private const string WorkingDirectory = @"..\..\..\Test\Integration";
        private readonly static string RepositoryDirectory = Path.Combine(WorkingDirectory, RepositoryName);
        private const string GitPath = @"..\..\..\Gits\{0}\bin\git.exe";
        private readonly static string ServerRepositoryPath = Path.Combine(@"..\..\..\Bonobo.Git.Server\App_Data\Repositories", RepositoryName);
        private readonly static string ServerRepositoryBackupPath = Path.Combine(@"..\..\..\Test\", RepositoryName, "Backup");
        private readonly static string[] GitVersions = { "1.7.4", "1.7.6", "1.7.7.1", "1.7.8", "1.7.9", "1.8.0", "1.8.1.2", "1.8.3", "1.9.5" };
        private readonly static string Credentials = "admin:admin@";
        private readonly static string RepositoryUrl = "http://{0}localhost:50287/Integration{1}";
        private readonly static string RepositoryUrlWithoutCredentials = String.Format(RepositoryUrl, String.Empty, String.Empty);
        private readonly static string RepositoryUrlWithCredentials = String.Format(RepositoryUrl, Credentials, ".git");

        private readonly static Dictionary<string, Dictionary<string, string>> Resources = new Dictionary<string, Dictionary<string, string>>
        {
            { String.Format(GitPath,  "1.7.4"), new Dictionary<string, string> 
            {
                { "Key", "Value" }
            }}
        };


        [TestInitialize]
        public void Initialize()
        {
            DeleteDirectory(WorkingDirectory);
        }

        [TestMethod, TestCategory(Definitions.Integration)]
        public void Run()
        {
            foreach (var version in GitVersions)
            {
                var git = String.Format(GitPath, version);
                var resources = new MsysgitResources(version);

                Directory.CreateDirectory(WorkingDirectory);
                BackupServerRepository();

                try
                {
                    CloneEmptyRepository(git, resources);
                    PushFiles(git, resources);
                    PushTag(git, resources);
                    PushBranch(git, resources);
                    CloneRepository(git, resources);
                    PullRepository(git, resources);
                    PullTag(git, resources);
                    PullBranch(git, resources);
                }
                finally
                {
                    RestoreServerRepository();
                    DeleteDirectory(WorkingDirectory);
                }
            }

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

        private void CloneEmptyRepository(string git, MsysgitResources resources)
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
