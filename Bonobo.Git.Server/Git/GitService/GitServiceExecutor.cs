using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

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
        private static readonly string[] _permittedServiceNames = {"upload-pack", "receive-pack"};
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
            if (!_permittedServiceNames.Contains(serviceName))
            {
                throw new ArgumentException("Invalid service name", nameof(serviceName));
            }

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

            var username = HttpContext.Current.User.Username();
            var teamsstr = "";
            var rolesstr = "";
            var displayname = "";
            if(!string.IsNullOrEmpty(username)){
                ITeamRepository tr = DependencyResolver.Current.GetService<ITeamRepository>();
                var userId = HttpContext.Current.User.Id();
                var teams = tr.GetTeams(userId);
                teamsstr = UserExtensions.StringlistToEscapedStringForEnvVar(teams.Select(x => x.Name));

                IRoleProvider rp = DependencyResolver.Current.GetService<IRoleProvider>();
                rolesstr = UserExtensions.StringlistToEscapedStringForEnvVar(rp.GetRolesForUser(userId));

                IMembershipService ms = DependencyResolver.Current.GetService<IMembershipService>();
                displayname = ms.GetUserModel(userId).DisplayName;

            }
            // If anonymous option is set then these will always be empty
            info.EnvironmentVariables.Add("AUTH_USER", username);
            info.EnvironmentVariables.Add("REMOTE_USER", username);
            info.EnvironmentVariables.Add("AUTH_USER_TEAMS", teamsstr);
            info.EnvironmentVariables.Add("AUTH_USER_ROLES", rolesstr);
            info.EnvironmentVariables.Add("AUTH_USER_DISPLAYNAME", displayname);


            using (var process = Process.Start(info))
            {
                //Do asynchronous copy i.e. spin up a separate task so we can simultaneously read/write &
                //avoid deadlock due to filled buffers within git process
                Task stdInTask = inStream.CopyToAsync(process.StandardInput.BaseStream);

                //Don't bother waiting on completion of stdOutTask while process is running.
                //task will be sitting idle waiting for new bytes on stdOut of git
                Task stdOutTask = process.StandardOutput.BaseStream.CopyToAsync(outStream);

                //wait for process death and ensure all data is sent and received
                bool completionJobDone = false;
                while (true)
                {
                    //check if all output has been sent to git exe via stdin
                    if ((stdInTask.IsCompleted) && (completionJobDone==false))
                    {
                        //all output has been sent to git process, send final signal.
                        if (options.endStreamWithClose)
                        {
                            process.StandardInput.Close();
                        }
                        else
                        {
                            process.StandardInput.Write('\0');
                        }
                        completionJobDone = true;
                    }

                    //check if git has terminated
                    if (process.HasExited)
                        break;

                    //lets not hog all the CPU, sleep for a little while (20ms)
                    Thread.Sleep(20);
                }
                stdOutTask.Wait();
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