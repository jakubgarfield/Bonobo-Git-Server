using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Diagnostics;

namespace Bonobo.Git.Server.Test
{
    [TestClass]
    public class MsysgitIntegrationTests
    {
        private const string RepositoryName = "Integration";
        private const string WorkingDirectory = @"D:\Desktop\Test\Integration";
        private readonly static string RepositoryDirectory = Path.Combine(WorkingDirectory, RepositoryName);
        private const string GitPath = @"D:\Projects\Bonobo Git Server\Other\Git\{0}\bin\git.exe";
        private readonly static string ServerRepositoryPath = Path.Combine(@"D:\Projects\Bonobo Git Server\Source\Bonobo.Git.Server\App_Data\Repositories", RepositoryName);
        private readonly static string ServerRepositoryBackupPath = Path.Combine(@"D:\Desktop\Test\", RepositoryName, "Backup");
        private readonly static string[] GitVersions = { "1.7.4" };
        private readonly static string Credentials = "admin:admin";
        private readonly static string RepositoryUrl = String.Format("http://{0}@localhost:50287/Integration.git", Credentials);


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

                Directory.CreateDirectory(WorkingDirectory);
                BackupServerRepository();

                try
                {
                    CloneEmptyRepository(git);
                    PushFiles(git);
                    PushTag(git);
                    PushBranch(git);
                    CloneRepository(git);
                    PullRepository(git);
                    PullTag(git);
                    PullBranch(git);
                }
                finally
                {
                    RestoreServerRepository();
                    DeleteDirectory(WorkingDirectory);
                }
            }

        }


        private void PullBranch(string git)
        {
        }

        private void PullTag(string git)
        {
        }

        private void PullRepository(string git)
        {
        }

        private void CloneRepository(string git)
        {
        }

        private void PushBranch(string git)
        {
            RunGit(git, "checkout -b \"TestBranch\"");
            var result = RunGit(git, "push origin TestBranch");
            
            Assert.AreEqual(String.Format("To {0}\n * [new branch]      TestBranch -> TestBranch\n", RepositoryUrl), result.Item2);
        }

        private void PushTag(string git)
        {
            RunGit(git, "tag -a v1.4 -m \"my version 1.4\"");
            var result = RunGit(git, "push --tags origin");
            
            Assert.AreEqual(String.Empty, result.Item1);
            Assert.AreEqual(String.Format("To {0}\n * [new tag]         v1.4 -> v1.4\n", RepositoryUrl), result.Item2);
        }

        private void PushFiles(string git)
        {
            CreateRandomFile(Path.Combine(RepositoryDirectory, "1.dat"), 10);
            CreateRandomFile(Path.Combine(RepositoryDirectory, "2.dat"), 1);
            Directory.CreateDirectory(Path.Combine(RepositoryDirectory, "SubDirectory"));
            CreateRandomFile(Path.Combine(RepositoryDirectory, "Subdirectory", "3.dat"), 20);
            CreateRandomFile(Path.Combine(RepositoryDirectory, "Subdirectory", "4.dat"), 15);

            RunGit(git, "add .");
            RunGit(git, "commit -m \"Test Files Added\"");
            var result = RunGit(git, "push origin master");
            
            Assert.AreEqual(String.Format("To {0}\n * [new branch]      master -> master\n", RepositoryUrl), result.Item2);
        }

        private void CloneEmptyRepository(string git)
        {
            var result = RunGit(git, String.Format(String.Format("clone {0}", RepositoryUrl), RepositoryName), WorkingDirectory);
            
            Assert.AreEqual("Cloning into Integration...\n", result.Item1);
            Assert.AreEqual("warning: You appear to have cloned an empty repository.\n", result.Item2);
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
