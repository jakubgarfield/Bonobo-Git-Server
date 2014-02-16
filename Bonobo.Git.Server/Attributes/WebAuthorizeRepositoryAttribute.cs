using System.Web.Mvc;
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

            var repository = filterContext.Controller.ControllerContext.RouteData.Values["id"].ToString();
            var user = filterContext.HttpContext.User.Identity.Name;
            if (RequiresRepositoryAdministrator)
            {
                if (!RepositoryPermissionService.IsRepositoryAdministrator(user, repository))
                {
                    filterContext.Result = new HttpUnauthorizedResult();
                }
            }
            else
            {
                if (RepositoryPermissionService.HasPermission(user, repository))
                {
                    return;
                }

                if (!RepositoryPermissionService.AllowsAnonymous(repository))
                {
                    filterContext.Result = new HttpUnauthorizedResult();
                }
            }
        }
    }
}