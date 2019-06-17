using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bonobo.Git.Server
{
    public class WebRequirement : IAuthorizationRequirement
    {

    }

    public class WebAuthorizationHandler : AuthorizationHandler<WebRequirement>
    {
        private string[] roles;

        public WebAuthorizationHandler()
        {

        }

        public string Roles
        {
            get
            {
                return roles == null ? null : String.Join(",", roles);
            }
            set
            {
                roles = value == null ? null : value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, WebRequirement requirement)
        {
            var redirectContext = context.Resource as AuthorizationFilterContext;
            if (!context.User.IsInRole(Definitions.Roles.Member) && !context.User.Identity.IsAuthenticated)
            {
                context.Fail();
                redirectContext.Result = new RedirectToActionResult("Unauthorized", "Home", null);
                return Task.CompletedTask;
            }

            if (roles != null && roles.Length != 0 && !context.User.Roles().Any(x => roles.Contains(x)))
            {
                context.Fail();
                redirectContext.Result = new RedirectToActionResult("Unauthorized", "Home", null);
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}