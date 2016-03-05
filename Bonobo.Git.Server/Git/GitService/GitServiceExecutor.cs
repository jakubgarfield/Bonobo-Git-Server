using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System;
using System.Collections.Generic;

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