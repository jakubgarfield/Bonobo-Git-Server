using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Routing;
using System.Security.Principal;
using Bonobo.Git.Server.Security;
using System.Configuration;

namespace Bonobo.Git.Server
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class WebAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {           
            var importer = new WindowsIdentityImporter();
            importer.Import(filterContext);

            if (IsWindowsUserAuthenticated(filterContext))
                return;

            if (filterContext.HttpContext.User == null || !(filterContext.HttpContext.User.Identity is FormsIdentity) || !filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Home" }, { "action", "LogOn" }, { "returnUrl", filterContext.HttpContext.Request.Url.PathAndQuery } });
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


        private bool IsWindowsUserAuthenticated(AuthorizationContext context)
        {
            var windowsIdentity = context.HttpContext.User.Identity as WindowsIdentity;
            return windowsIdentity != null && windowsIdentity.IsAuthenticated;
        }
    }
}