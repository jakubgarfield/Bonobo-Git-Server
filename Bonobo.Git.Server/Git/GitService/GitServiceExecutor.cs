using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService
{
    // perhaps can be done cleaner than this, but i can't figure out how to
    // to do registration in Unity to handle 2 string constructor parameters
    public class GitServiceExecutorParams
    {
        public string GitPath { get; set; }
        public string RepositoriesDirPath { get; set; }
    }


    public class GitServiceExecutor : IGitService
    {
        private readonly string gitPath;
        private readonly string repositoriesDirPath;
        private readonly IGitRepositoryLocator repoLocator;

        public GitServiceExecutor(GitServiceExecutorParams parameters, IGitRepositoryLocator repoLocator)
        {
            this.gitPath = parameters.GitPath;
            this.repositoriesDirPath = parameters.RepositoriesDirPath;
            this.repoLocator = repoLocator;
        }

        public void ExecuteServiceByName(string repositoryName, string serviceName, ExecutionOptions options, System.IO.Stream inStream, System.IO.Stream outStream)
        {
            var args = serviceName + " --stateless-rpc";
            args += options.ToCommandLineArgs();
            args += " \"" + repoLocator.GetRepositoryDirectoryPath(repositoryName).FullName + "\"";

            var info = new System.Diagnostics.ProcessStartInfo(gitPath, args)
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(this.repositoriesDirPath),
            };

            using (var process = System.Diagnostics.Process.Start(info))
            {
                inStream.CopyTo(process.StandardInput.BaseStream);
                process.StandardInput.Write('\0');

                var buffer = new byte[16 * 1024];
                int read;
                while ((read = process.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outStream.Write(buffer, 0, read);
                    outStream.Flush();
                }

                process.WaitForExit();
            }
        }
    }
}