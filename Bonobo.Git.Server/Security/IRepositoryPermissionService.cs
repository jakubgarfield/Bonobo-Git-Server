using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Security
{
    public interface IRepositoryPermissionService
    {
        // Used by bonobo
        bool HasPermission(Guid userId, Guid repositoryId);
        bool IsRepositoryAdministrator(Guid userId, Guid repositoryId);
        bool AllowsAnonymous(Guid repositoryId);
        IEnumerable<RepositoryModel> GetAllPermittedRepositories(Guid userId);

        // Used by git clients as they don't have the GUID of the project
        bool HasPermission(Guid userId, string repositoryName);
        bool AllowsAnonymous(string repositoryName);
        
    }
}