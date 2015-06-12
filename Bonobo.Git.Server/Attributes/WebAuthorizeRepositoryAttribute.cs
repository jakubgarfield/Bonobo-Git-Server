using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
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

            if (filterContext.HttpContext.User == null || !(filterContext.HttpContext.User.Identity is FormsIdentity) || !filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result =
                    new RedirectToRouteResult(new RouteValueDictionary
                    {
                        { "controller", "Home" },
                        { "action", "LogOn" },
                        { "returnUrl", filterContext.HttpContext.Request.Url.PathAndQuery }
                    });
            }
            else
            {
                filterContext.Result = new RedirectResult("~/Home/Unauthorized");
            }
        }
    }
}