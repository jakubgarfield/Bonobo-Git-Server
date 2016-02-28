using System;
using System.Linq;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;

using Microsoft.Practices.Unity;

namespace Bonobo.Git.Server.Security
{
    public class ADRepositoryPermissionService : IRepositoryPermissionService
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
            try
            {
                return Repository.GetRepository(repositoryId).AnonymousAccess;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public bool HasPermission(Guid userId, string repositoryName)
        {
            var repository = Repository.GetRepository(repositoryName);
            return repository != null && HasPermission(userId, repository.Id);
        }

        public bool HasPermission(Guid userId, Guid repositoryId)
        {
            bool result = false;
            RepositoryModel repositoryModel;
            try
            {
                repositoryModel = Repository.GetRepository(repositoryId);
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            result |= repositoryModel.Users.Any(x => x.Id == userId);
            result |= repositoryModel.Administrators.Any(x => x.Id == userId);
            result |= RoleProvider.GetRolesForUser(userId).Contains(Definitions.Roles.Administrator);
            result |= TeamRepository.GetTeams(userId).Any(x => repositoryModel.Teams.Select(y => y.Name).Contains(x.Name, StringComparer.OrdinalIgnoreCase));

            return result;
        }

        public bool IsRepositoryAdministrator(Guid userId, Guid repositoryId)
        {
            bool result = false;

            result |= Repository.GetRepository(repositoryId).Administrators.Any(x => x.Id == userId);
            result |= RoleProvider.GetRolesForUser(userId).Contains(Definitions.Roles.Administrator);

            return result;
        }
    }
}