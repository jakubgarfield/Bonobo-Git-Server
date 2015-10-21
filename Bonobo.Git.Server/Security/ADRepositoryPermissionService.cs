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

        public bool AllowsAnonymous(string repositoryName)
        {
            return Repository.GetRepository(repositoryName).AnonymousAccess;
        }

        public bool HasPermission(string username, string repositoryName)
        {
            bool result = false;

            RepositoryModel repositoryModel = Repository.GetRepository(repositoryName);

            result |= repositoryModel.Users.Contains(username, StringComparer.OrdinalIgnoreCase);
            result |= repositoryModel.Administrators.Contains(username, StringComparer.OrdinalIgnoreCase);
            result |= RoleProvider.GetRolesForUser(username).Contains(Definitions.Roles.Administrator);
            result |= TeamRepository.GetTeams(username).Any(x => repositoryModel.Teams.Contains(x.Name, StringComparer.OrdinalIgnoreCase));

            return result;
        }

        public bool IsRepositoryAdministrator(string username, string repositoryName)
        {
            bool result = false;

            result |= Repository.GetRepository(repositoryName).Administrators.Contains(username, StringComparer.OrdinalIgnoreCase);
            result |= RoleProvider.GetRolesForUser(username).Contains(Definitions.Roles.Administrator);

            return result;
        }
    }
}