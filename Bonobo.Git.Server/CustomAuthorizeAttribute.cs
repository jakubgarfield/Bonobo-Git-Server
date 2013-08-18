using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace Bonobo.Git.Server
{
    public abstract class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var windowsIdentity = filterContext.HttpContext.User.Identity as WindowsIdentity;
            if (windowsIdentity == null)
            {
                CustomAuthenticate(filterContext);
            }
            else if (windowsIdentity.IsAuthenticated)
            {
                var importer = new WindowsIdentityImporter();
                importer.Import(windowsIdentity);
            }
        }

        protected abstract void CustomAuthenticate(AuthorizationContext filterContext);
    }
}