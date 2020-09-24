using Bonobo.Git.Server.Security;
using System;
using System.Web.Mvc;
using Unity;

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
                Guid repoId;
                var urlhelper = new UrlHelper(filterContext.RequestContext);
                if (Guid.TryParse(filterContext.Controller.ControllerContext.RouteData.Values["id"].ToString(), out repoId))
                {
                    Guid userId = filterContext.HttpContext.User.Id();

                    var requiredAccess = RequiresRepositoryAdministrator
                        ? RepositoryAccessLevel.Administer
                        : RepositoryAccessLevel.Push;

                    if (RepositoryPermissionService.HasPermission(userId, repoId, requiredAccess))
                    {
                        return;
                    }

                    filterContext.Result = new RedirectResult(urlhelper.Action("Unauthorized", "Home"));
                }
                else
                {
                    var rd = filterContext.RequestContext.RouteData;
                    var action = rd.GetRequiredString("action");
                    var controller = rd.GetRequiredString("controller");
                    if (action.Equals("index", StringComparison.OrdinalIgnoreCase) && controller.Equals("repository", StringComparison.OrdinalIgnoreCase))
                    {
                        filterContext.Result = new RedirectResult(urlhelper.Action("Unauthorized", "Home"));
                    }
                    else
                    {
                        filterContext.Controller.TempData["RepositoryNotFound"] = true;
                        filterContext.Result = new RedirectResult(urlhelper.Action("Index", "Repository"));
                    }
                }
            }
        }
    }
}
