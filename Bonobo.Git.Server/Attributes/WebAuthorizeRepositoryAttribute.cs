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

        public bool AllowAnonymousAccessWhenRepositoryAllowsIt { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            Guid repoId = Guid.Empty;
            UrlHelper urlhelper = null;

            // is this set to allow anon users?
            if (AllowAnonymousAccessWhenRepositoryAllowsIt)
            {
                urlhelper = GetUrlHelper(filterContext);
                repoId = GetRepoId(filterContext);
                //if the user is authenciated or the repo id isnt there let the normal auth code handle it.
                if (repoId!=Guid.Empty && !filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    //we are only allowing read access here.  The web ui doesnt do pushes
                    if(RepositoryPermissionService.HasPermission(Guid.Empty,repoId, RepositoryAccessLevel.Pull))
                    {
                        return;
                    }
                }
            }
            //do base role checks
            base.OnAuthorization(filterContext);

            if (!(filterContext.Result is HttpUnauthorizedResult))
            {
                if (urlhelper == null)
                {
                    urlhelper = GetUrlHelper(filterContext);
                }
                if (repoId==Guid.Empty)                {
                    repoId = GetRepoId(filterContext);
                }
                if (repoId!=Guid.Empty)
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
        private static Guid GetRepoId(AuthorizationContext filterContext)
        {
            Guid result;
            if (Guid.TryParse(filterContext.Controller.ControllerContext.RouteData.Values["id"].ToString(), out result))
            {
                return result;
            }
            else
            {
                return Guid.Empty;
            }
        }

        private static UrlHelper GetUrlHelper(AuthorizationContext filterContext)
        {
            return new UrlHelper(filterContext.RequestContext);
        }
    }
}
