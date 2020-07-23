using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Bonobo.Git.Server.Models;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Security;
using System.Threading;
using Microsoft.Practices.Unity;
using Bonobo.Git.Server.Helpers;
using Serilog;

namespace Bonobo.Git.Server.Data
{
    public sealed class ADBackend
    {
        [Dependency]
        public IMembershipService MembershipService { get; set; }

        public ADBackendStore<RepositoryModel> Repositories { get { return repositories.Value; } }
        public ADBackendStore<TeamModel> Teams { get { return teams.Value; } }
        public ADBackendStore<UserModel> Users { get { return users.Value; } }
        public ADBackendStore<RoleModel> Roles { get { return roles.Value; } }
        public static ADBackend Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            instance = new ADBackend(true);
                        }
                    }
                }
                return instance;
            }
        }

        public static void ResetSingletonForTesting()
        {
            ResetSingletonWithoutAutomaticUpdate();
        }

        public static void ResetSingletonWithoutAutomaticUpdate()
        {
            lock (instanceLock)
            {
                instance = new ADBackend(false);
            }
        }

        public static void ResetSingletonWithAutomaticUpdate()
        {
            lock (instanceLock)
            {
                instance = new ADBackend(true);
            }
        }

        private Lazy<ADBackendStore<RepositoryModel>> repositories = new Lazy<ADBackendStore<RepositoryModel>>(() =>
        {
            return new ADBackendStore<RepositoryModel>(ActiveDirectorySettings.BackendPath, "Repos");
        });

        private Lazy<ADBackendStore<TeamModel>> teams = new Lazy<ADBackendStore<TeamModel>>(() =>
        {
            return new ADBackendStore<TeamModel>(ActiveDirectorySettings.BackendPath, "Teams");
        });

        private Lazy<ADBackendStore<UserModel>> users = new Lazy<ADBackendStore<UserModel>>(() =>
        {
            return new ADBackendStore<UserModel>(ActiveDirectorySettings.BackendPath, "Users");
        });

        private Lazy<ADBackendStore<RoleModel>> roles = new Lazy<ADBackendStore<RoleModel>>(() =>
        {
            return new ADBackendStore<RoleModel>(ActiveDirectorySettings.BackendPath, "Roles");
        });

        private static volatile ADBackend instance;
        private static object instanceLock = new object();
        private object updateLock = new object();
        private Timer updateTimer;

        private ADBackend(bool enableAutoUpdate)
        {
            if (enableAutoUpdate)
            {
                updateTimer = new Timer(Update, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(180));
            }
        }

        private void Update(object state)
        {
            if (Monitor.TryEnter(updateLock))
            {
                try
                {
                    UpdateUsers();
                    UpdateTeams();
                    UpdateRoles();
                    UpdateRepositories();
                }
                catch(Exception ex)
                {
                    Log.Error(ex, "Failed to update data from AD");
                }
                finally
                {
                    Monitor.Exit(updateLock);
                }
            }
        }

        private UserModel GetUserModelFromPrincipal(UserPrincipal user)
        {
            UserModel result = null;

            try
            {
                if (user != null)
                {
                    result = new UserModel
                    {
                        Id = user.Guid.Value,
                        Username = user.UserPrincipalName,
                        GivenName = user.GivenName ?? String.Empty,
                        Surname = user.Surname ?? String.Empty,
                        Email = user.EmailAddress ?? String.Empty,
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to convert UserPrincipal to UserModel");
            }

            return result;
        }

        private void UpdateRepositories()
        {
            foreach(RepositoryModel repository in Repositories)
            {
                UserModel[] usersToRemove = repository.Users.Where(repoUser => !Users.Select(u => u.Id).Contains(repoUser.Id)).ToArray();
                TeamModel[] teamsToRemove = repository.Teams.Where(repoTeam => !Teams.Select(team => team.Id).Contains(repoTeam.Id)).ToArray();
                repository.Users = repository.Users.Except(usersToRemove).ToArray();
                repository.Teams = repository.Teams.Except(teamsToRemove).ToArray();
                if (usersToRemove.Length > 0 || teamsToRemove.Length > 0)
                {
                    Repositories.Update(repository);
                }
            }
        }

        private void UpdateUsers()
        {
            try
            {
                GroupPrincipal   group;
                PrincipalContext pc = ADHelper.GetMembersGroup(out group);

                foreach (Guid Id in Users.Select(x => x.Id).Where(x => ADHelper.GetUserPrincipal(x) == null))
                {
                    Users.Remove(Id);
                }

                foreach (string username in ADHelper.GetGroupMembers(group).OfType<UserPrincipal>().Select(x => x.UserPrincipalName).Where(x => x != null))
                {
                    UserPrincipal principal = ADHelper.GetUserPrincipal(username);
                    UserModel user = GetUserModelFromPrincipal(principal);
                    if (user != null)
                    {
                        Users.AddOrUpdate(user);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AD: Failed to update users.");
            }
        }

        private void UpdateTeams()
        {
            foreach (var team in Teams.Select(x => new { x.Id, Name = x.Name }).Where(x => !ActiveDirectorySettings.TeamNameToGroupNameMapping.Keys.Contains(x.Name, StringComparer.OrdinalIgnoreCase)))
            {
                Teams.Remove(team.Id);
            }

            if(MembershipService == null)
                MembershipService = new ADMembershipService();

            foreach (string teamName in ActiveDirectorySettings.TeamNameToGroupNameMapping.Keys)
            {
                string groupName = ActiveDirectorySettings.TeamNameToGroupNameMapping[teamName];

                Log.Verbose("AD: Updating team {TeamName} (groupName {GroupName})", teamName, groupName);
                try
                {
                    GroupPrincipal group;
                    PrincipalContext pc = ADHelper.GetPrincipalGroup(groupName, out group);
                    TeamModel teamModel = new TeamModel()
                    {
                        Id = group.Guid.Value,
                        Description = group.Description,
                        Name = teamName,
                        Members = ADHelper.GetGroupMembers(group).Select(x => MembershipService.GetUserModel(x.Guid.Value)).Where(o => o != null).ToArray()
                    };
                    Teams.AddOrUpdate(teamModel);
                    Log.Verbose("AD: Updated team {TeamName} OK", teamName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "AD: Failed to update team {TeamName}", teamName);
                }
            }
        }

        private void UpdateRoles()
        {
            foreach (var role in Roles.Select(x => new { x.Id, Name = x.Name }).Where(x => !ActiveDirectorySettings.RoleNameToGroupNameMapping.Keys.Contains(x.Name, StringComparer.OrdinalIgnoreCase)))
            {
                Roles.Remove(role.Id);
            }
            
            foreach (string roleName in ActiveDirectorySettings.RoleNameToGroupNameMapping.Keys)
            {
                string groupName = ActiveDirectorySettings.RoleNameToGroupNameMapping[roleName];
                Log.Verbose("AD: Updating role {RoleName} (groupName {GroupName})", roleName, groupName);
                try
                {
                    GroupPrincipal group;
                    PrincipalContext pc = ADHelper.GetPrincipalGroup(groupName, out group);
                    RoleModel roleModel = new RoleModel
                    {
                        Id = group.Guid.Value,
                        Name = roleName,
                        Members = ADHelper.GetGroupMembers(group).Where(x => x is UserPrincipal).Select(x => x.Guid.Value)
                                .ToArray()
                    };
                    Roles.AddOrUpdate(roleModel);
                    Log.Verbose("AD: Updated role {RoleName} OK", roleName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "AD: Failed to update role {roleName}", roleName);
                }
            }
        }
    }
}