using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Bonobo.Git.Server.Security;

namespace Bonobo.Git.Server
{
    public class FormsAuthorizeRepositoryAttribute : FormsAuthorizeAttribute
    {
        [Dependency]
        public IRepositoryPermissionService RepositoryPermissionService { get; set; }

        public bool RequiresRepositoryAdministrator { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var repository = filterContext.Controller.ControllerContext.RouteData.Values["id"].ToString();
            var user = filterContext.HttpContext.User.Identity.Name;
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

            base.OnAuthorization(filterContext);
        }
    }
}