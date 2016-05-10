using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

using Bonobo.Git.Server.Models;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Security;
using System.Threading;
using Microsoft.Practices.Unity;

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
            lock (instanceLock)
            {
                instance = new ADBackend(false);
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
                    LogException(ex);
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
                LogException(ex);
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
            var gitUsersByDomain = Users.ToLookup(x => x.Username.GetDomain(), StringComparer.OrdinalIgnoreCase);
            try
            {
                ILookup<string, string> groupUsersByDomain;
                using (PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, ActiveDirectorySettings.DefaultDomain))
                using (GroupPrincipal memberGroup = GetMembersGroup(principalContext))
                {
                    groupUsersByDomain = memberGroup.GetMembers(true).OfType<UserPrincipal>().Select(x => x.UserPrincipalName).Where(x => x != null).ToLookup(x=>x.GetDomain(), StringComparer.OrdinalIgnoreCase);
                }

                var allDomains = groupUsersByDomain.Select(x=>x.Key).Union(gitUsersByDomain.Select(x=>x.Key)).Distinct(StringComparer.OrdinalIgnoreCase);

                foreach (var domain in allDomains)
                {
                    var gitUsers = (gitUsersByDomain.Contains(domain) ? gitUsersByDomain[domain].ToArray() : new UserModel[] {}).ToDictionary(x=>x.Username, StringComparer.OrdinalIgnoreCase);
                    var groupUsers = new HashSet<string>(groupUsersByDomain.Contains(domain) ? groupUsersByDomain[domain].ToArray() : new string[] {});

                    foreach (var gitUser in gitUsers.Values.Where(x=>!groupUsers.Contains(x.Username)))
                    {
                        Users.Remove(gitUser);
                    }

                    using (PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, domain))
                    { 
                        foreach (var groupUser in groupUsers)
                        {
                            using (var principal = UserPrincipal.FindByIdentity(principalContext, IdentityType.UserPrincipalName, groupUser))
                            {
                                UserModel user = GetUserModelFromPrincipal(principal);
                                if (user != null)
                                {
                                    Users.AddOrUpdate(user);
                                }
                                else if(gitUsers.ContainsKey(groupUser))
                                {
                                    Users.Remove(gitUsers[groupUser]);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
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

            using (PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, ActiveDirectorySettings.DefaultDomain))
            {
                foreach (string teamName in ActiveDirectorySettings.TeamNameToGroupNameMapping.Keys)
                {
                    try
                    {
                        using (GroupPrincipal group = GroupPrincipal.FindByIdentity(principalContext, IdentityType.Name, ActiveDirectorySettings.TeamNameToGroupNameMapping[teamName]))
                        {
                            TeamModel teamModel = new TeamModel() {
                                Id = group.Guid.Value,
                                Description = group.Description,
                                Name = teamName,
                                Members = group.GetMembers(true).Select(x => MembershipService.GetUserModel(x.Guid.Value)).ToArray()
                            };
                            if (teamModel != null)
                            {
                                Teams.AddOrUpdate(teamModel);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                    }
                }
            }
        }

        private void UpdateRoles()
        {
            foreach (var role in Roles.Select(x => new { x.Id, Name = x.Name }).Where(x => !ActiveDirectorySettings.RoleNameToGroupNameMapping.Keys.Contains(x.Name, StringComparer.OrdinalIgnoreCase)))
            {
                Roles.Remove(role.Id);
            }

            PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, ActiveDirectorySettings.DefaultDomain);
            foreach (string roleName in ActiveDirectorySettings.RoleNameToGroupNameMapping.Keys)
            {
                GroupPrincipal group = GroupPrincipal.FindByIdentity(principalContext, IdentityType.Name, ActiveDirectorySettings.RoleNameToGroupNameMapping[roleName]);

                RoleModel roleModel = new RoleModel()
                {
                    Id = group.Guid.Value,
                    Name = roleName,
                    Members = group.GetMembers(true).Where(x => x is UserPrincipal).Select(x => x.Guid.Value).ToArray()
                };
                Roles.AddOrUpdate(roleModel);
            }
        }

        private void LogException(Exception exception)
        {
            Trace.TraceError("{0}: ADBackend Exception: {1}", DateTime.Now, exception);
        }
    }
}
