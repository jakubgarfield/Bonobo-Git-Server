using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bonobo.Git.Server
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AuthorizeRedirectAttribute : AuthorizeAttribute
    {
        public string RedirectUrl
        {
            get;
            set;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (filterContext.Result is HttpUnauthorizedResult)
            {
                string redirectUrl = RedirectUrl;
                if (String.IsNullOrEmpty(redirectUrl))
                {
                    redirectUrl = "~/Home/Unauthorized";
                }
                filterContext.Result = new RedirectResult(redirectUrl);
            }
        }
    }
}