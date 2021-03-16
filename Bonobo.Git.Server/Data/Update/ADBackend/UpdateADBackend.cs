using Bonobo.Git.Server.Data.Update.Pre600ADBackend;
using System.IO;
using Bonobo.Git.Server.Helpers;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using Bonobo.Git.Server.Configuration;
using System;
using System.Threading;
using System.Linq;
using Bonobo.Git.Server.Data;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Serilog;

namespace Bonobo.Git.Server.Data.Update.ADBackendUpdate
{
    public class Pre600UpdateTo600
    {
        // Before 6.0.0 the mapping was done via the name property. After that the Guid is used.
        public static void UpdateADBackend()
        {
            // Make a copy of the current backendfolder if it exists, so we can use the modern models for saving
            // it all to the correct location directly
            var backendDirectory = PathEncoder.GetRootPath(ActiveDirectorySettings.BackendPath);
            var backupDirectory = PathEncoder.GetRootPath(ActiveDirectorySettings.BackendPath + "_pre6.0.0_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"));
            if (Directory.Exists(backendDirectory) && BackEndNeedsUpgrade(backendDirectory))
            {
                MakeBackupOfBackendDirectory(backendDirectory, backupDirectory);

                // We must create one that will not automatically update the items while we update them
                ADBackend.ResetSingletonWithoutAutomaticUpdate();

                var newUsers = UpdateUsers(Path.Combine(backupDirectory, "Users"));
                var newTeams = UpdateTeams(Path.Combine(backupDirectory, "Teams"), newUsers);
                UpdateRoles(Path.Combine(backupDirectory, "Roles"), newUsers);
                UpdateRepos(Path.Combine(backupDirectory, "Repos"), newUsers, newTeams);

                // We are done, enable automatic update again.
                ADBackend.ResetSingletonWithAutomaticUpdate();
            }
        }

        /// <summary>
        /// Check if the backend needs upgrading - try to load all the .json files - if they load OK and have GUID ids, then 
        /// we don't need to load
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        private static bool BackEndNeedsUpgrade(string dir)
        {
            if (BackendSubDirectoryNeedsUpdating<Models.RoleModel>(dir, "Roles"))
            {
                return true;
            }
            if (BackendSubDirectoryNeedsUpdating<Models.UserModel>(dir, "Users"))
            {
                return true;
            }
            if (BackendSubDirectoryNeedsUpdating<Models.TeamModel>(dir, "Teams"))
            {
                return true;
            }
            if (BackendSubDirectoryNeedsUpdating<Models.RepositoryModel>(dir, "Repos"))
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Try all the json files in one subdirectory
        /// </summary>
        private static bool BackendSubDirectoryNeedsUpdating<T>(string backendDirectory, string subdirectory) where T : INameProperty
        {
            var directory = Path.Combine(backendDirectory, subdirectory);
            foreach (var jsonfile in new DirectoryInfo(directory).EnumerateFiles("*.json"))
            {
                // try to load with the modern models, if it succeeds we don't need to update
                try
                {
                    var x = JsonConvert.DeserializeObject<T>(File.ReadAllText(jsonfile.FullName));
                    if (x.Id == Guid.Empty)
                    {
                        // No GUID - we need to convert
                        return true;
                    }
                    break;
                }
                catch (JsonSerializationException)
                {
                    // We must convert...
                    return true;
                }
            }
            return false;
        }

        private static void MakeBackupOfBackendDirectory(string backendDirectory, string backupDirectory)
        {
            FileSystem.CopyDirectory(backendDirectory, backupDirectory);
            int attemptsRemaining = 5;
            while (attemptsRemaining-- > 0)
            {
                try
                {
                    Directory.Delete(backendDirectory, true);
                    return;
                }
                catch (IOException) // System.IO.IOException: The directory is not empty
                {
                    // Normally it's only the top most dir that fails to delete for some reason (usually because Explorer is holding a handle to stuff)...
                    // If any json files are left we have a problem, if only folders are left
                    // we can let it update without any problems. Leaving the old json files
                    // means they would get loaded next run and crash the server.
                    if (attemptsRemaining == 0 && Directory.EnumerateFiles(backendDirectory, "*.json").Any())
                    {
                        throw;
                    }
                    Thread.Sleep(1000);
                }
            }
        }

        private static void UpdateRepos(string dir, Dictionary<string, Models.UserModel> users, Dictionary<string, Models.TeamModel> teams)
        {
            var repos = Pre600Functions.LoadContent<Pre600RepositoryModel>(dir);
            foreach(var repoitem in repos)
            {
                var repo = repoitem.Value;
                var newrepo = new Models.RepositoryModel();
                newrepo.Id = Guid.NewGuid();
                newrepo.Name = repo.Name;
                newrepo.Group = repo.Group;
                newrepo.Description = repo.Description;
                newrepo.AnonymousAccess = repo.AnonymousAccess;
                newrepo.AuditPushUser = repo.AuditPushUser;
                newrepo.Logo = repo.Logo;
                newrepo.RemoveLogo = repo.RemoveLogo;

                var list = new List<Models.UserModel>();
                foreach (var user in repo.Users)
                {
                    list.Add(users[user]);
                }
                newrepo.Users = list.ToArray();

                list.Clear();
                foreach(var admins in repo.Administrators)
                {
                    list.Add(users[admins]);
                }
                newrepo.Administrators = list.ToArray();

                var newteams = new List<Models.TeamModel>();
                foreach(var team in repo.Teams)
                {
                    newteams.Add(teams[team]);
                }
                newrepo.Teams = newteams.ToArray();

                ADBackend.Instance.Repositories.Add(newrepo);
            }
        }

        private static void UpdateRoles(string dir, Dictionary<string, Models.UserModel> users)
        {
            var roles = Pre600Functions.LoadContent<Pre600RoleModel>(dir);
            foreach(var roleitem in roles)
            {
                var role = roleitem.Value;
                var newrole = new Models.RoleModel();
                newrole.Name = role.Name;

                newrole.Id = Guid.NewGuid();
                var members = new List<Guid>();
                foreach (var memberName in role.Members)
                {
                    if (memberName == null)
                    {
                        Log.Warning("Role {0} in file {1} contained member with value \"null\". Skipping this member.", role.Name, dir);
                        continue;
                    }

                    Models.UserModel user;
                    if (users.TryGetValue(memberName, out user))
                    {
                        members.Add(user.Id);
                    }
                }
                newrole.Members = members.ToArray();
            }
        }

        private static Dictionary<string, Models.TeamModel> UpdateTeams(string dir, Dictionary<string, Models.UserModel> users)
        {
            var teams = Pre600Functions.LoadContent<Pre600TeamModel>(dir);
            var newTeams = new Dictionary<string, Models.TeamModel>();
            
            foreach (var teamitem in teams)
            {
                var team = teamitem.Value;
                var newteam = new Models.TeamModel();
                newteam.Name = team.Name;
                newteam.Description = team.Description;
                newteam.Id = Guid.NewGuid();

                try
                {
                    GroupPrincipal group;
                    PrincipalContext pc = ADHelper.GetPrincipalGroup(ActiveDirectorySettings.TeamNameToGroupNameMapping[team.Name], out group);

                    newteam.Id = group.Guid.Value;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to acquire group GUID for teamName {team} - adding new.", team.Name);
                }

                
                var members = new List<Models.UserModel>();
                foreach (var member in team.Members)
                {
                    members.Add(users[member]);
                }
                newteam.Members = members.ToArray();

                ADBackend.Instance.Teams.Add(newteam);
                newTeams[team.Name] = newteam;
            }
            return newTeams;
        }

        private static Dictionary<string, Models.UserModel> UpdateUsers(string dir)
        {
            var users = Pre600Functions.LoadContent<Pre600UserModel>(dir);
            var domains = new Dictionary<string, PrincipalContext>();
            var newUsers = new Dictionary<string, Models.UserModel>();
            foreach (var user in users)
            {
                var ou = user.Value;
                var u = new Models.UserModel();
                u.Email = ou.Email;
                u.GivenName = ou.GivenName;
                u.Surname = ou.Surname;
                u.Username = ou.Name;
                u.Id = Guid.NewGuid();

                var domainuser = ADHelper.GetUserPrincipal(u.Username);
                if (domainuser != null && domainuser.Guid.HasValue)
                {
                    u.Id = domainuser.Guid.Value;
                }

                ADBackend.Instance.Users.Add(u);
                newUsers[u.Username] = u;
            }
            return newUsers;
        }
    }
}
