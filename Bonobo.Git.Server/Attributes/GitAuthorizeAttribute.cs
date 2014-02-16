using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Security.Principal;
using Bonobo.Git.Server.Security;
using Microsoft.Practices.Unity;

namespace Bonobo.Git.Server
{
    public class GitAuthorizeAttribute : AuthorizeAttribute
    {
        [Dependency]
        public IMembershipService MembershipService { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (IsWindowsUserAuthenticated(filterContext))
            {
                return;
            }

            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            string auth = filterContext.HttpContext.Request.Headers["Authorization"];

            if (String.IsNullOrEmpty(auth))
            {
                return;
            }

            byte[] encodedDataAsBytes = Convert.FromBase64String(auth.Replace("Basic ", String.Empty));
            string value = Encoding.ASCII.GetString(encodedDataAsBytes);
            string username = value.Substring(0, value.IndexOf(':'));
            string password = value.Substring(value.IndexOf(':') + 1);

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password) &&
                MembershipService.ValidateUser(username, password))
            {
                filterContext.HttpContext.User = new GenericPrincipal(new GenericIdentity(username), null);
            }
            else
            {
                filterContext.Result = new HttpStatusCodeResult(401);
            }
        }


        private static bool IsWindowsUserAuthenticated(ControllerContext context)
        {
            var windowsIdentity = context.HttpContext.User.Identity as WindowsIdentity;
            return windowsIdentity != null && windowsIdentity.IsAuthenticated;
        }
    }
}