using System;
using Bonobo.Git.Server.Extensions;
using Bonobo.Git.Server.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace Bonobo.Git.Server
{
    public class WebAuthorizeRepositoryAttribute : WebAuthorizeAttribute
    {
        public bool RequiresRepositoryAdministrator { get; set; }

        public bool AllowAnonymousAccessWhenRepositoryAllowsIt { get; set; }

        public override void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            var repositoryPermissionService = filterContext.HttpContext.RequestServices.GetService<IRepositoryPermissionService>();

            Guid repoId = Guid.Empty;
            UrlHelper urlhelper = null;

            // is this set to allow anon users?
            if (AllowAnonymousAccessWhenRepositoryAllowsIt)
            {
                //urlhelper = GetUrlHelper(filterContext);
                repoId = GetRepoId(filterContext);
                //if the user is authenciated or the repo id isnt there let the normal auth code handle it.
                if (repoId != Guid.Empty && !filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    //we are only allowing read access here.  The web ui doesnt do pushes
                    if (repositoryPermissionService.HasPermission(Guid.Empty, repoId, RepositoryAccessLevel.Pull))
                    {
                        return;
                    }
                }
            }
            //do base role checks
            base.OnAuthorization(filterContext);

            if (!(filterContext.Result is UnauthorizedResult))
            {
                if (urlhelper == null)
                {
                    urlhelper = GetUrlHelper(filterContext);
                }
                if (repoId == Guid.Empty)
                {
                    repoId = GetRepoId(filterContext);
                }
                if (repoId != Guid.Empty)
                {
                    Guid userId = filterContext.HttpContext.User.Id();

                    var requiredAccess = RequiresRepositoryAdministrator
                        ? RepositoryAccessLevel.Administer
                        : RepositoryAccessLevel.Push;

                    if (repositoryPermissionService.HasPermission(userId, repoId, requiredAccess))
                    {
                        return;
                    }

                    filterContext.Result = new RedirectResult(urlhelper.Action("Unauthorized", "Home"));
                }
                else
                {
                    var rd = filterContext.RouteData;
                    var action = rd.GetRequiredString("action");
                    var controller = rd.GetRequiredString("controller");
                    if (action.Equals("index", StringComparison.OrdinalIgnoreCase) && controller.Equals("repository", StringComparison.OrdinalIgnoreCase))
                    {
                        filterContext.Result = new RedirectResult(urlhelper.Action("Unauthorized", "Home"));
                    }
                    else
                    {
                        ITempDataProvider tempDataProvider = filterContext.HttpContext.RequestServices.GetService<ITempDataProvider>();
                        var tempData = tempDataProvider.LoadTempData(filterContext.HttpContext);
                        tempData["RepositoryNotFound"] = true;
                        tempDataProvider.SaveTempData(filterContext.HttpContext, tempData);
                        filterContext.Result = new RedirectResult(urlhelper.Action("Index", "Repository"));
                    }
                }
            }
        }
        private static Guid GetRepoId(AuthorizationFilterContext filterContext)
        {
            Guid result;
            if (Guid.TryParse(filterContext.RouteData.Values["id"].ToString(), out result))
            {
                return result;
            }
            else
            {
                return Guid.Empty;
            }
        }

        private static UrlHelper GetUrlHelper(AuthorizationFilterContext filterContext)
        {
            var actionContextAccessor = filterContext.HttpContext.RequestServices.GetService<IActionContextAccessor>();
            var actionContext = actionContextAccessor.ActionContext;
            return new UrlHelper(actionContext);
        }
    }
}
