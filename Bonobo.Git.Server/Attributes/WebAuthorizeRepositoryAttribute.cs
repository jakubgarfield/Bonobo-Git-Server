using System.Web.Mvc;
using System.Web.Routing;

using Bonobo.Git.Server.Security;

using Microsoft.Practices.Unity;

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
                string repository = filterContext.Controller.ControllerContext.RouteData.Values["id"].ToString();
                string user = filterContext.HttpContext.User.Id();

                if (filterContext.HttpContext.User.IsInRole(Definitions.Roles.Administrator))
                {
                    return;
                }

                if (RequiresRepositoryAdministrator)
                {
                    if (RepositoryPermissionService.IsRepositoryAdministrator(user, repository))
                    {
                        return;
                    }
                }
                else
                {
                    if (RepositoryPermissionService.HasPermission(user, repository))
                    {
                        return;
                    }

                    if (RepositoryPermissionService.AllowsAnonymous(repository))
                    {
                        return;
                    }
                }

                filterContext.Result = new RedirectResult("~/Home/Unauthorized");
            }
        }
    }
}