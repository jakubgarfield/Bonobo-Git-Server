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
    public class BasicAuthorizeAttribute : AuthorizeAttribute
    {
        [Dependency]
        public IMembershipService MembershipService { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            string auth = filterContext.HttpContext.Request.Headers["Authorization"];

            if (!String.IsNullOrEmpty(auth))
            {
                byte[] encodedDataAsBytes = Convert.FromBase64String(auth.Replace("Basic ", ""));
                string value = Encoding.ASCII.GetString(encodedDataAsBytes);
                string username = value.Substring(0, value.IndexOf(':'));
                string password = value.Substring(value.IndexOf(':') + 1);

                if (MembershipService.ValidateUser(username, password))
                {
                    filterContext.HttpContext.User = new GenericPrincipal(new GenericIdentity(username), null);
                }
                else
                {
                    filterContext.Result = new HttpUnauthorizedResult();
                }
            }
            else
            {
                if (AuthorizeCore(filterContext.HttpContext))
                {
                    HttpCachePolicyBase cachePolicy = filterContext.HttpContext.Response.Cache;
                    cachePolicy.SetProxyMaxAge(new TimeSpan(0));
                    cachePolicy.AddValidationCallback(CacheValidateHandler, null);
                }
                else
                {
                    filterContext.HttpContext.Response.Clear();
                    filterContext.HttpContext.Response.StatusCode = 401;
                    filterContext.HttpContext.Response.StatusDescription = "Unauthorized";
                    filterContext.HttpContext.Response.AddHeader("WWW-Authenticate", "Basic realm=\"Secure Area\"");
                    filterContext.HttpContext.Response.Write("401, please authenticate");
                    filterContext.HttpContext.Response.End();
                    filterContext.Result = new HttpUnauthorizedResult();
                }
            }
        }

        private void CacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
        {
            validationStatus = OnCacheAuthorization(new HttpContextWrapper(context));
        }
    }
}