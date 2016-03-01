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

        public bool RequiresRepositoryAdministrator { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            if (!(filterContext.Result is HttpUnauthorizedResult))
            {
                Guid repoId = Guid.Parse(filterContext.Controller.ControllerContext.RouteData.Values["id"].ToString());
                Guid userId = filterContext.HttpContext.User.Id();

                var requiredAccess = RequiresRepositoryAdministrator
                    ? RepositoryAccessLevel.Administer
                    : RepositoryAccessLevel.Push;

                if (RepositoryPermissionService.HasPermission(userId, repoId, requiredAccess))
                {
                    return;
                }

                filterContext.Result = new RedirectResult("~/Home/Unauthorized");
            }
        }
    }
}
