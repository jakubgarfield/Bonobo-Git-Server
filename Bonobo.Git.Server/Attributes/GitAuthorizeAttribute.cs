using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.DirectoryServices.AccountManagement;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;

using Microsoft.Practices.Unity;

namespace Bonobo.Git.Server
{
    public class GitAuthorizeAttribute : AuthorizeAttribute
    {
        [Dependency]
        public IMembershipService MembershipService { get; set; }

        [Dependency]
        public IAuthenticationProvider AuthenticationProvider { get; set; }

        [Dependency]
        public IRepositoryPermissionService RepositoryPermissionService { get; set; }

        [Dependency]
        public IRepositoryRepository RepositoryRepository { get; set; }

        public static string GetRepoPath(string path, string applicationPath)
        {
            var repo = path.Replace(applicationPath, "").Replace("/","");
            return repo.Substring(0, repo.IndexOf(".git"));
        }

        public override void OnAuthorization(System.Web.Mvc.AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            HttpContextBase httpContext = filterContext.HttpContext;

            string incomingRepoName = GetRepoPath(httpContext.Request.Path, httpContext.Request.ApplicationPath);
            string repo = Repository.NormalizeRepositoryName(incomingRepoName, RepositoryRepository);

            // check if repo allows anonymous access - if we need more access than 'Pull', this will be checked later anyway
            //WD - I think there may be a problem here - if the repo is anon-pull, but permission-push, then short-circuiting
            // the auth at this stage will prevent the client from ever being told to authorise
            // We probably need get the isPush test up out of the GitController in some way
            if (RepositoryPermissionService.HasPermission(Guid.Empty, repo, RepositoryAccessLevel.Pull))
            {
                return;
            }

            if (httpContext.Request.IsAuthenticated && httpContext.User != null && httpContext.User.Identity is System.Security.Claims.ClaimsIdentity)
            {
                return;
            }

            // Add header to prevent redirection to login page (see IAuthenticationProvider.Configure)
            httpContext.Request.Headers.Add("AuthNoRedirect", "1");
            string auth = httpContext.Request.Headers["Authorization"];

            if (String.IsNullOrEmpty(auth))
            {
                httpContext.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Bonobo Git\"");
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                return;
            }

            byte[] encodedDataAsBytes = Convert.FromBase64String(auth.Replace("Basic ", String.Empty));
            string value = Encoding.ASCII.GetString(encodedDataAsBytes);
            string username = Uri.UnescapeDataString(value.Substring(0, value.IndexOf(':')));
            string password = Uri.UnescapeDataString(value.Substring(value.IndexOf(':') + 1));

            bool allowed = false;

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                if (AuthenticationProvider is WindowsAuthenticationProvider)
                {
                    var domain = username.GetDomain();
                    username = username.StripDomain();
                    try
                    {
                        using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domain))
                        {
                            var adUser = UserPrincipal.FindByIdentity(pc, username);
                            if (adUser != null)
                            {
                                if (pc.ValidateCredentials(username, password))
                                {
                                    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(AuthenticationProvider.GetClaimsForUser(username.Replace("\\", "!"))));
                                    allowed = true;
                                }
                            }
                        }
                    }
                    catch (PrincipalException)
                    {
                        // let it fail
                    }
                }
                else
                {
                    if (MembershipService.ValidateUser(username, password) == ValidationResult.Success)
                    {
                        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(AuthenticationProvider.GetClaimsForUser(username)));
                        allowed = true;
                    }
                }
            }

            if (!allowed)
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }
        }
    }
}