using System.Linq;
using Bonobo.Git.Server.Data;
using System;
using System.Collections.Generic;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Models;
using Microsoft.Practices.Unity;

namespace Bonobo.Git.Server.Security
{
    public class RepositoryPermissionService : IRepositoryPermissionService
    {
        [Dependency]
        public IRepositoryRepository Repository { get; set; }

        [Dependency]
        public IRoleProvider RoleProvider { get; set; }

        [Dependency]
        public ITeamRepository TeamRepository { get; set; }
        
        public bool HasPermission(Guid userId, string repositoryName, RepositoryAccessLevel requiredLevel)
        {
            var repository = Repository.GetRepository(repositoryName);
            return repository != null && HasPermission(userId, repository.Id, requiredLevel);
        }

        public bool HasPermission(Guid userId, Guid repositoryId, RepositoryAccessLevel requiredLevel)
        {
            return HasPermission(userId, TeamRepository.GetTeams(userId), Repository.GetRepository(repositoryId), requiredLevel);
        }

        private bool HasPermission(Guid userId, IList<TeamModel> userTeams, RepositoryModel repositoryModel, RepositoryAccessLevel requiredLevel)
        {
            if (userId == Guid.Empty)
            {
                // This is an anonymous user, the rules are different
                return CheckAnonymousPermission(repositoryModel, requiredLevel);
            }
            else
            {
                return CheckNamedUserPermission(userId, userTeams, repositoryModel, requiredLevel);
            }
        }

        public bool HasCreatePermission(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                // Anonymous users cannot create repos
                return false;
            }
            return IsSystemAdministrator(userId) || UserConfiguration.Current.AllowUserRepositoryCreation;
        }

        public IEnumerable<RepositoryModel> GetAllPermittedRepositories(Guid userId, RepositoryAccessLevel requiredLevel)
        {
            // This is just an optimisation to avoid expensive permission checks which will always pass for sysadmins
            // Even if it was removed, the correct rules would be applied in HasPermission, but the IsSysAdmin check would need to be done for every repo
            var isSystemAdministrator = IsSystemAdministrator(userId);
            var userTeams = TeamRepository.GetTeams(userId);
            return Repository.GetAllRepositories().Where(repo => isSystemAdministrator || HasPermission(userId, userTeams, repo, requiredLevel));
        }

        private bool CheckAnonymousPermission(RepositoryModel repository, RepositoryAccessLevel requiredLevel)
        {
            if (!repository.AnonymousAccess)
            {
                // There's no anon access at all to this repo
                return false;
            }

            switch (requiredLevel)
            {
                case RepositoryAccessLevel.Pull:
                    return true;
                case RepositoryAccessLevel.Push:
                    return repository.AllowAnonymousPush == RepositoryPushMode.Yes || (repository.AllowAnonymousPush == RepositoryPushMode.Global && UserConfiguration.Current.AllowAnonymousPush);
                case RepositoryAccessLevel.Administer:
                    // No anonymous administrators ever
                    return false;
                default:
                    throw new ArgumentOutOfRangeException("requiredLevel", requiredLevel, null);
            }
        }

        private bool CheckNamedUserPermission(Guid userId, IList<TeamModel> userTeams, RepositoryModel repository, RepositoryAccessLevel requiredLevel)
        {
            if (userId == Guid.Empty) { throw new ArgumentException("Do not pass anonymous user id", "userId"); }

            bool userIsAnAdministrator = IsAnAdminstrator(userId, repository);

            switch (requiredLevel)
            {
                case RepositoryAccessLevel.Push:
                case RepositoryAccessLevel.Pull:
                    return userIsAnAdministrator || 
                        UserIsARepoUser(userId, repository) || 
                        UserIsATeamMember(userTeams, repository);

                case RepositoryAccessLevel.Administer:
                    return userIsAnAdministrator;
                default:
                    throw new ArgumentOutOfRangeException("requiredLevel", requiredLevel, null);
            }
        }

        private static bool UserIsARepoUser(Guid userId, RepositoryModel repository)
        {
            return repository.Users.Any(x => x.Id == userId);
        }

        private static bool UserIsATeamMember(IList<TeamModel> userTeams, RepositoryModel repository)
        {
            return userTeams
                .Any(x => repository.Teams.Select(y => y.Name).Contains(x.Name, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Check if a user can administer this repo - either by being sysAdmin, or by being on the repo's own admin list
        /// </summary>
        private bool IsAnAdminstrator(Guid userId, RepositoryModel repositoryModel)
        {
            return repositoryModel.Administrators.Any(x => x.Id == userId)
                || IsSystemAdministrator(userId);
        }

        private bool IsSystemAdministrator(Guid userId)
        {
            return RoleProvider.GetRolesForUser(userId).Contains(Definitions.Roles.Administrator);
        }
    }
}