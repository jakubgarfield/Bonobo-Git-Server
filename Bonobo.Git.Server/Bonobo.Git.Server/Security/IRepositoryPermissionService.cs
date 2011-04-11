using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Security
{
    public interface IRepositoryPermissionService
    {
        bool HasPermission(string username, string project);
        bool AllowsAnonymous(string project);
        bool IsRepositoryAdministrator(string username, string project);
    }
}