using System.Web.Mvc;
using System.Web.Routing;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;

using Microsoft.Practices.Unity;
using System;

namespace Bonobo.Git.Server
{
    public class WebAuthorizeRepositoryAttribute : WebAuthorizeAttribute
    {
        [Dependency]
        public IRepositoryPermissionService RepositoryPermissionService { get; set; }

        [Dependency]
        public IRepositoryRepository RepositoryRepository { get; set; }

        public bool RequiresRepositoryAdministrator { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            if (!(filterContext.Result is HttpUnauthorizedResult))
            {
                Guid incomingRepoId = Guid.Parse(filterContext.Controller.ControllerContext.RouteData.Values["id"].ToString());
                Guid userId = filterContext.HttpContext.User.Id();

                if (filterContext.HttpContext.User.IsInRole(Definitions.Roles.Administrator))
                {
                    return;
                }

                if (RequiresRepositoryAdministrator)
                {
                    if (RepositoryPermissionService.IsRepositoryAdministrator(userId, incomingRepoId))
                    {
                        return;
                    }
                }
                else
                {
                    if (RepositoryPermissionService.HasPermission(userId, incomingRepoId))
                    {
                        return;
                    }

                    if (RepositoryPermissionService.AllowsAnonymous(incomingRepoId))
                    {
                        return;
                    }
                }

                filterContext.Result = new RedirectResult("~/Home/Unauthorized");
            }
        }
    }
}
