using System.Diagnostics;
using System.IO;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService
{
    // perhaps can be done cleaner than this, but i can't figure out how to
    // to do registration in Unity to handle 2 string constructor parameters
    public class GitServiceExecutorParams
    {
        public string GitPath { get; set; }
        
        public string GitHomePath { get; set; }
        
        public string RepositoriesDirPath { get; set; }
    }

    public class GitServiceExecutor : IGitService
    {
        private readonly string gitPath;
        private readonly string gitHomePath;
        private readonly string repositoriesDirPath;
        private readonly IGitRepositoryLocator repoLocator;

        public GitServiceExecutor(GitServiceExecutorParams parameters, IGitRepositoryLocator repoLocator)
        {
            this.gitPath = parameters.GitPath;
            this.gitHomePath = parameters.GitHomePath;
            this.repositoriesDirPath = parameters.RepositoriesDirPath;
            this.repoLocator = repoLocator;
        }

        public void ExecuteServiceByName(
            string correlationId,
            string repositoryName,
            string serviceName,
            ExecutionOptions options,
            Stream inStream,
            Stream outStream)
        {
            var args = serviceName + " --stateless-rpc";
            args += options.ToCommandLineArgs();
            args += " \"" + repoLocator.GetRepositoryDirectoryPath(repositoryName).FullName + "\"";

            var info = new ProcessStartInfo(gitPath, args)
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(repositoriesDirPath),
            };

            SetHomePath(info);
            info.EnvironmentVariables.Add("AUTH_USER", HttpContext.Current.Request.ServerVariables["AUTH_USER"]);

            using (var process = Process.Start(info))
            {
                inStream.CopyTo(process.StandardInput.BaseStream);
                if (options.endStreamWithClose) {
                    process.StandardInput.Close();
                } else {
                    process.StandardInput.Write('\0');
                }

                process.StandardOutput.BaseStream.CopyTo(outStream);
                process.WaitForExit();
            }
        }

        private void SetHomePath(ProcessStartInfo info)
        {
            if (info.EnvironmentVariables.ContainsKey("HOME"))
            {
                info.EnvironmentVariables.Remove("HOME");
            }
            info.EnvironmentVariables.Add("HOME", gitHomePath);
        }
    }
}