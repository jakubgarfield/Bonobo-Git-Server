using System.Web.Mvc;
using System.Web.Routing;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;

using Microsoft.Practices.Unity;

namespace Bonobo.Git.Server
{
    public class WebAuthorizeRepositoryAttribute : WebAuthorizeAttribute
    {
        [Dependency]
        public IRepositoryPermissionService RepositoryPermissionService { get; set; }

        [Dependency]
        public IRepositoryRepository RepositoryRepository { get; set; }

        public bool RequiresRepositoryAdministrator { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            if (!(filterContext.Result is HttpUnauthorizedResult))
            {
                string incomingRepoName = filterContext.Controller.ControllerContext.RouteData.Values["id"].ToString();
                string repository = Repository.NormalizeRepositoryName(incomingRepoName, RepositoryRepository);

                string user = filterContext.HttpContext.User.Username();

                if (filterContext.HttpContext.User.IsInRole(Definitions.Roles.Administrator))
                {
                    return;
                }

                if (RequiresRepositoryAdministrator)
                {
                    if (RepositoryPermissionService.IsRepositoryAdministrator(user, repository))
                    {
                        return;
                    }
                }
                else
                {
                    if (RepositoryPermissionService.HasPermission(user, repository))
                    {
                        return;
                    }

                    if (RepositoryPermissionService.AllowsAnonymous(repository))
                    {
                        return;
                    }
                }

                filterContext.Result = new RedirectResult("~/Home/Unauthorized");
            }
        }
    }
}
