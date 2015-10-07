using System;
using System.Linq;
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
            base.OnAuthorization(filterContext);

            if (!(filterContext.Result is HttpUnauthorizedResult))
            {
                if (!filterContext.HttpContext.User.IsInRole(Definitions.Roles.Member))
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