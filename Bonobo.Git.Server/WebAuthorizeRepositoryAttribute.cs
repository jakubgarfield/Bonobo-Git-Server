using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Bonobo.Git.Server.Security;
using Bonobo.Git.Server.Extensions;

namespace Bonobo.Git.Server
{
    public class WebAuthorizeRepositoryAttribute : WebAuthorizeAttribute
    {
        [Dependency]
        public IRepositoryPermissionService RepositoryPermissionService { get; set; }

        public bool RequiresRepositoryAdministrator { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            var repository = filterContext.Controller.ControllerContext.RouteData.Values["id"].ToString();
            var user = filterContext.HttpContext.User.GetUsername();
            if (RequiresRepositoryAdministrator)
            {
                if (!RepositoryPermissionService.IsRepositoryAdministrator(user, repository))
                {
                    filterContext.Result = new HttpUnauthorizedResult();
                }
            }
            else
            {
                if (!RepositoryPermissionService.HasPermission(user, repository))
                {
                    if (!RepositoryPermissionService.AllowsAnonymous(repository))
                    {
                        filterContext.Result = new HttpUnauthorizedResult();
                    }
                }
            }

        }
    }
}