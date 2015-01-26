using System;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace Bonobo.Git.Server
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class WebAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            WindowsIdentityImporter.Import(filterContext);
            FormsIdentityImporter.Import(filterContext);

            if (IsWindowsUserAuthenticated(filterContext))
            {
                return;
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
                base.OnAuthorization(filterContext);
                if (filterContext.Result is HttpUnauthorizedResult)
                {
                    filterContext.Result = new RedirectResult("~/Home/Unauthorized");
                }
            }
        }

        private static bool IsWindowsUserAuthenticated(ControllerContext context)
        {
            var windowsIdentity = context.HttpContext.User.Identity as WindowsIdentity;
            var rv = windowsIdentity != null && windowsIdentity.IsAuthenticated;
            if (!rv)
            {
                var formsIdentity = context.HttpContext.User.Identity as FormsIdentity;
                rv = formsIdentity != null && formsIdentity.IsAuthenticated;
            }

            return rv;
        }
    }
}