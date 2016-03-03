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
            var repositoryModel = Repository.GetRepository(repositoryId);

            if (userId == Guid.Empty)
            {
                // This is an anonymous user, the rules are different
                return CheckAnonymousPermission(repositoryModel, requiredLevel);
            }
            else
            {
                return CheckNamedUserPermission(userId, repositoryModel, requiredLevel);
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
            return Repository.GetAllRepositories().Where(repo => HasPermission(userId, repo.Id, requiredLevel));
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
                    return UserConfiguration.Current.AllowAnonymousPush;
                case RepositoryAccessLevel.Administer:
                    // No anonymous administrators ever
                    return false;
                default:
                    throw new ArgumentOutOfRangeException("requiredLevel", requiredLevel, null);
            }
        }

        private bool CheckNamedUserPermission(Guid userId, RepositoryModel repository, RepositoryAccessLevel requiredLevel)
        {
            if (userId == Guid.Empty) { throw new ArgumentException("Do not pass anonymous user id", "userId"); }

            bool userIsAnAdministrator = IsAnAdminstrator(userId, repository);
            var userIsATeamMember =
                TeamRepository.GetTeams(userId)
                    .Any(x => repository.Teams.Select(y => y.Name).Contains(x.Name, StringComparer.OrdinalIgnoreCase));
            var userIsARepoUser = repository.Users.Any(x => x.Id == userId);

            switch (requiredLevel)
            {
                case RepositoryAccessLevel.Push:
                case RepositoryAccessLevel.Pull:
                    return userIsARepoUser || userIsATeamMember || userIsAnAdministrator;

                case RepositoryAccessLevel.Administer:
                    return userIsAnAdministrator;
                default:
                    throw new ArgumentOutOfRangeException("requiredLevel", requiredLevel, null);
            }
        }

        /// <summary>
        /// Check if a user can administer this repo - either by being sysAdmin, or by being on the repo's own admin list
        /// </summary>
        private bool IsAnAdminstrator(Guid userId, RepositoryModel repositoryModel)
        {
            bool result = false;

            result |= repositoryModel.Administrators.Any(x => x.Id == userId);
            result |= IsSystemAdministrator(userId);

            return result;
        }

        private bool IsSystemAdministrator(Guid userId)
        {
            return RoleProvider.GetRolesForUser(userId).Contains(Definitions.Roles.Administrator);
        }
    }
}