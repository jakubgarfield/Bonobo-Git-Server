using System.Linq;
using Bonobo.Git.Server.Data;
using System;
using System.Collections.Generic;
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
        
        public bool AllowsAnonymous(string repositoryName)
        {
            var repository = Repository.GetRepository(repositoryName);
            return repository != null && repository.AnonymousAccess;
        }

        public bool AllowsAnonymous(Guid repositoryId)
        {
            return Repository.GetRepository(repositoryId).AnonymousAccess;
        }

        public bool HasPermission(Guid userId, string repositoryName)
        {
            var repository = Repository.GetRepository(repositoryName);
            return repository != null && HasPermission(userId, repository.Id);
        }

        public bool HasPermission(Guid userId, Guid repositoryId)
        {
            bool result = false;
            var repositoryModel = Repository.GetRepository(repositoryId);

            if (repositoryModel.AnonymousAccess)
            {
                // Don't try and check any user stuff if we allow anon access
                return true;
            }

            result |= repositoryModel.Users.Any(x => x.Id == userId);
            result |= repositoryModel.Administrators.Any(x => x.Id == userId);
            result |= IsSystemAdministrator(userId);
            result |= TeamRepository.GetTeams(userId).Any(x => repositoryModel.Teams.Select(y => y.Name).Contains(x.Name, StringComparer.OrdinalIgnoreCase));

            return result;
        }

        public bool IsRepositoryAdministrator(Guid userId, Guid repositoryId)
        {
            bool result = false;

            result |= Repository.GetRepository(repositoryId).Administrators.Any(x => x.Id == userId);
            result |= IsSystemAdministrator(userId);

            return result;
        }

        public IEnumerable<RepositoryModel> GetAllPermittedRepositories(Guid userId)
        {
            return Repository.GetAllRepositories().Where(repo => HasPermission(userId, repo.Id));
        }

        private bool IsSystemAdministrator(Guid userId)
        {
            return RoleProvider.GetRolesForUser(userId).Contains(Definitions.Roles.Administrator);
        }
    }
}