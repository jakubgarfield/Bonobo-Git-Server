using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Security
{
    public interface IRepositoryPermissionService
    {
        // Used by bonobo
        bool HasPermission(Guid userId, Guid repositoryId);
        bool IsRepositoryAdministrator(Guid userId, Guid repositoryId);
        bool AllowsAnonymous(Guid repositoryId);

        // Used by git clients as they don't have the GUID of the project
        bool HasPermission(Guid userId, string repositoryName);
        bool HasPermission(string username, string password, string repositoryName);
        bool AllowsAnonymous(string repositoryName);
        
    }
}