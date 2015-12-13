using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Bonobo.Git.Server.Models;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;
using Bonobo.Git.Server.Configuration;
using System.Threading;

namespace Bonobo.Git.Server.Data
{
    public sealed class ADBackend
    {
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
                            instance = new ADBackend();
                        }
                    }
                }

                return instance;
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

        private ADBackend()
        {
            updateTimer = new Timer(Update, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(180));
        }

        private void Update(object state)
        {
            if (Monitor.TryEnter(updateLock))
            {
                try
                {
                    Parallel.Invoke(() => UpdateUsers(), () => UpdateTeams(), () => UpdateRoles());
                    UpdateRepositories();
                }
                catch
                {
                }
                finally
                {
                    Monitor.Exit(updateLock);
                }
            }
        }

        private GroupPrincipal GetMembersGroup(PrincipalContext principalContext)
        {
            GroupPrincipal result = null;

            if (!String.IsNullOrEmpty(ActiveDirectorySettings.MemberGroupName))
            {
                result = GroupPrincipal.FindByIdentity(principalContext, IdentityType.Name, ActiveDirectorySettings.MemberGroupName);
            }

            return result;
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
                        Name = user.UserPrincipalName,
                        GivenName = user.GivenName ?? String.Empty,
                        Surname = user.Surname ?? String.Empty,
                        Email = user.EmailAddress ?? String.Empty,
                    };
                }
            }
            catch
            {
            }

            return result;
        }

        private void UpdateRepositories()
        {
            foreach(RepositoryModel repository in Repositories)
            {
                string[] usersToRemove = repository.Users.Where(x => !Users.Select(u => u.Name).Contains(x, StringComparer.OrdinalIgnoreCase)).ToArray();
                string[] teamsToRemove = repository.Teams.Where(x => !Teams.Select(u => u.Name).Contains(x, StringComparer.OrdinalIgnoreCase)).ToArray();
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
                using (PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, ActiveDirectorySettings.DefaultDomain))
                using (GroupPrincipal memberGroup = GetMembersGroup(principalContext))
                {
                    foreach (string user in Users.Select(x => x.Name).Where(x => UserPrincipal.FindByIdentity(principalContext, IdentityType.UserPrincipalName, x) == null))
                    {
                        Users.Remove(user);
                    }

                    foreach (string username in memberGroup.GetMembers(true).OfType<UserPrincipal>().Select(x => x.UserPrincipalName).Where(x => x != null))
                    {
                        using (UserPrincipal principal = UserPrincipal.FindByIdentity(principalContext, IdentityType.UserPrincipalName, username))
                        {
                            UserModel user = GetUserModelFromPrincipal(principal);
                            if (user != null)
                            {
                                Users.AddOrUpdate(user);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void UpdateTeams()
        {
            foreach (string team in Teams.Select(x => x.Name).Where(x => !ActiveDirectorySettings.TeamNameToGroupNameMapping.Keys.Contains(x, StringComparer.OrdinalIgnoreCase)))
            {
                Teams.Remove(team);
            }

            using (PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, ActiveDirectorySettings.DefaultDomain))
            {
                foreach (string teamName in ActiveDirectorySettings.TeamNameToGroupNameMapping.Keys)
                {
                    try
                    {
                        using (GroupPrincipal group = GroupPrincipal.FindByIdentity(principalContext, IdentityType.Name, ActiveDirectorySettings.TeamNameToGroupNameMapping[teamName]))
                        {
                            TeamModel teamModel = new TeamModel() { Description = group.Description, Name = teamName, Members = group.GetMembers(true).Select(x => x.UserPrincipalName).ToArray() };
                            if (teamModel != null)
                            {
                                Teams.AddOrUpdate(teamModel);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void UpdateRoles()
        {
            foreach (string role in Roles.Select(x => x.Name).Where(x => !ActiveDirectorySettings.RoleNameToGroupNameMapping.Keys.Contains(x, StringComparer.OrdinalIgnoreCase)))
            {
                Roles.Remove(role);
            }

            PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, ActiveDirectorySettings.DefaultDomain);
            foreach (string roleName in ActiveDirectorySettings.RoleNameToGroupNameMapping.Keys)
            {
                GroupPrincipal group = GroupPrincipal.FindByIdentity(principalContext, IdentityType.Name, ActiveDirectorySettings.RoleNameToGroupNameMapping[roleName]);
                RoleModel roleModel = new RoleModel()
                {
                    Name = roleName,
                    Members = group.GetMembers(true).Where(x => x is UserPrincipal).Select(x => x.UserPrincipalName).ToArray()
                };
                Roles.AddOrUpdate(roleModel);
            }
        }
    }
}