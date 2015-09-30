using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web.Routing;

namespace Bonobo.Git.Server
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class WebAuthorizeAttribute : AuthorizeAttribute
    {
        public new string Roles
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

        private string[] roles;

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var importer = new WindowsIdentityImporter();
            WindowsIdentityImporter.Import(filterContext);

            if (IsWindowsUserAuthenticated(filterContext))
            {
                return;
            }

            if (filterContext.HttpContext.User == null || !(filterContext.HttpContext.User.Identity is ClaimsIdentity) || !filterContext.HttpContext.User.Identity.IsAuthenticated)
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
                base.OnAuthorization(filterContext);
                if (filterContext.Result is HttpUnauthorizedResult || (roles != null && !filterContext.HttpContext.User.Roles().Any(x => roles.Contains(x))))
                {
                    filterContext.Result = new RedirectResult("~/Home/Unauthorized");
                }
            }
        }

        private static bool IsWindowsUserAuthenticated(ControllerContext context)
        {
            var windowsIdentity = context.HttpContext.User.Identity as WindowsIdentity;
            return windowsIdentity != null && windowsIdentity.IsAuthenticated;
        }
    }
}