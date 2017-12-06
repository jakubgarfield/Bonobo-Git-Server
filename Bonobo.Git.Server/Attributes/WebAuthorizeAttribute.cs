using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bonobo.Git.Server
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class WebAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public new string Roles
        {
            get
            {
                return roles == null ? null : string.Join(",", roles);
            }
            set
            {
                roles = value?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        private string[] roles;


        public virtual void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (!(filterContext.Result is Microsoft.AspNetCore.Mvc.UnauthorizedResult))
            {
                if (!filterContext.HttpContext.User.IsInRole(Definitions.Roles.Member) && !filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    filterContext.Result = new RedirectResult("~/Home/Unauthorized");
                }

                if (roles != null && roles.Length != 0 && !filterContext.HttpContext.User.Roles().Any(x => roles.Contains(x)))
                {
                    filterContext.Result = new RedirectResult("~/Home/Unauthorized");
                }
            }
        }
    }
}