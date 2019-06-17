using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

using Bonobo.Git.Server.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Bonobo.Git.Server
{
    public class WebRepositoryAuthorizationHandler : WebAuthorizationHandler
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        public IRepositoryPermissionService RepositoryPermissionService { get; set; }

        public bool RequiresRepositoryAdministrator { get; set; }

        public WebRepositoryAuthorizationHandler(IActionContextAccessor actionContextAccessor, IRepositoryPermissionService repositoryPermissionService)
        {
            _actionContextAccessor = actionContextAccessor;
            RepositoryPermissionService = repositoryPermissionService;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, WebRequirement requirement)
        {
            Guid repoId;
            var urlhelper = new UrlHelper(_actionContextAccessor.ActionContext);
            if (Guid.TryParse(_actionContextAccessor.ActionContext.RouteData.Values["id"].ToString(), out repoId))
            {
                Guid userId = context.User.Id();

                var requiredAccess = RequiresRepositoryAdministrator
                    ? RepositoryAccessLevel.Administer
                    : RepositoryAccessLevel.Push;

                if (RepositoryPermissionService.HasPermission(userId, repoId, requiredAccess))
                {
                    context.Succeed(requirement);
                    return Task.FromResult(0);
                }

                //filterContext.Result = new RedirectResult(urlhelper.Action("Unauthorized", "Home"));
            }
            else
            {
                var rd = _actionContextAccessor.ActionContext.RouteData;
                rd.Values.TryGetValue("action", out var action);
                rd.Values.TryGetValue("controller", out var controller);
                if (((string)action).Equals("index", StringComparison.OrdinalIgnoreCase) && ((string)controller).Equals("repository", StringComparison.OrdinalIgnoreCase))
                {
                    //filterContext.Result = new RedirectResult(urlhelper.Action("Unauthorized", "Home"));
                }
                else
                {
                    //filterContext.Controller.TempData["RepositoryNotFound"] = true;
                    //filterContext.Result = new RedirectResult(urlhelper.Action("Index", "Repository"));
                }
            }
            return Task.FromResult(0);
        }
    }
}
