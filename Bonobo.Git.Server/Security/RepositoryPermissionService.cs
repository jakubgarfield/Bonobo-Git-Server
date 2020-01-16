using System.Linq;
using Bonobo.Git.Server.Data;
using System;
using System.Collections.Generic;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Models;
using Microsoft.Practices.Unity;
using Serilog;

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
            return HasPermission(userId, TeamRepository.GetTeams(userId), IsSystemAdministrator(userId), Repository.GetRepository(repositoryId), requiredLevel);
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
            var userIsSystemAdministrator = IsSystemAdministrator(userId);
            var userTeams = TeamRepository.GetTeams(userId);
            return Repository.GetAllRepositories().Where(repo => HasPermission(userId, userTeams, userIsSystemAdministrator, repo, requiredLevel));
        }

        public bool UserIsAdministratorInRepository(Guid userId, string repositoryName)
        {
            var repository = Repository.GetRepository(repositoryName);
            return repository.Administrators.Any(x => x.Id == userId);

        }

        private bool HasPermission(Guid userId, IList<TeamModel> userTeams, bool userIsSystemAdministrator,
            RepositoryModel repositoryModel, RepositoryAccessLevel requiredLevel)
        {
            // All users can take advantage of the anonymous permissions
            if (CheckAnonymousPermission(repositoryModel, requiredLevel))
            {
                Log.Verbose(
                    "RepoPerms: Permitting user {UserId} anonymous permission {Permission} on repo {RepositoryName}",
                    userId,
                    requiredLevel,
                    repositoryModel.Name);
                return true;
            }
            if (userId == Guid.Empty)
            {
                // We have no named user
                return false;
            }

            // Named users have more possibilities
            return CheckNamedUserPermission(userId, userTeams, userIsSystemAdministrator, repositoryModel, requiredLevel);
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

        private bool CheckNamedUserPermission(Guid userId, IList<TeamModel> userTeams, bool userIsSystemAdministrator, RepositoryModel repository, RepositoryAccessLevel requiredLevel)
        {
            if (userId == Guid.Empty) { throw new ArgumentException("Do not pass anonymous user id", "userId"); }

            bool userIsAnAdministrator = userIsSystemAdministrator || repository.Administrators.Any(x => x.Id == userId);

            Log.Verbose("RepoPerms: Checking user {UserId} (admin? {IsAdmin}) has permission {Permission} on repo {RepositoryName}",
                userId,
                userIsAnAdministrator,
                requiredLevel,
                repository.Name);

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

        private bool IsSystemAdministrator(Guid userId)
        {
            return RoleProvider.GetRolesForUser(userId).Contains(Definitions.Roles.Administrator);
        }
    }
}