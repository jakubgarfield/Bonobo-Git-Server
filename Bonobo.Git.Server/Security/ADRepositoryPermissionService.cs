using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        
        [Dependency]
        public IMembershipService MemberShipService { get; set; } 

        public bool AllowsAnonymous(string repositoryName)
        {
            return Repository.GetRepository(repositoryName).AnonymousAccess;
        }

        public bool AllowsAnonymous(Guid repositoryId)
        {
            return Repository.GetRepository(repositoryId).AnonymousAccess;
        }

        public bool HasPermission(Guid userId, string repositoryName)
        {
            return HasPermission(userId, Repository.GetRepository(repositoryName).Id);
        }

        public bool HasPermission(Guid userId, Guid repositoryId)
        {
            bool result = false;

            RepositoryModel repositoryModel = Repository.GetRepository(repositoryId);
            UserModel user = MemberShipService.GetUserModel(userId);

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