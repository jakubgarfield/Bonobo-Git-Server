using System;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.DirectoryServices.AccountManagement;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;
using Microsoft.Practices.Unity;
using Bonobo.Git.Server.Helpers;
using Serilog;

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
            string repoName = Repository.NormalizeRepositoryName(incomingRepoName, RepositoryRepository);

            // Add header to prevent redirection to login page even if we fail auth later (see IAuthenticationProvider.Configure)
            // If we don't fail auth later, then this is benign
            httpContext.Request.Headers.Add("AuthNoRedirect", "1");

            if (httpContext.Request.IsAuthenticated && httpContext.User != null && httpContext.User.Identity is System.Security.Claims.ClaimsIdentity)
            {
                // We already have a claims id, we don't need to worry about the rest of these checks
                Log.Verbose("GitAuth: User {username} already has identity", httpContext.User.DisplayName());
                return;
            }

            string authHeader = httpContext.Request.Headers["Authorization"];

            if (String.IsNullOrEmpty(authHeader))
            {
                // We don't have an auth header, but if we're doing an anonymous operation, that's OK
                if (RepositoryPermissionService.HasPermission(Guid.Empty, repoName, RepositoryAccessLevel.Pull))
                {
                    // Allow this through.  If it turns out they're actually trying to do an anon push and that's not allowed for this repo
                    // then the GitController will reject them in there
                    Log.Information("GitAuth: No auth header, anon operation may be allowed");
                    return;
                }
                else
                {
                    // If we're not even allowed to do an anonymous pull, then we should bounce this now, 
                    // and tell the other end to try again with an auth header included next time
                    httpContext.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Bonobo Git\"");
                    filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

                    Log.Information("GitAuth: No auth header, anon operations not allowed");
                    return;
                }
            }

            // Process the auth header and see if we've been given valid credentials
            if (!IsUserAuthorized(authHeader, httpContext))
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }
        }

        private bool IsUserAuthorized(string authHeader, HttpContextBase httpContext)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(authHeader.Replace("Basic ", String.Empty));
            string value = Encoding.ASCII.GetString(encodedDataAsBytes);
            string username = Uri.UnescapeDataString(value.Substring(0, value.IndexOf(':')));
            string password = Uri.UnescapeDataString(value.Substring(value.IndexOf(':') + 1));

            Log.Information("GitAuth: Trying to auth user {username}", username);

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                Log.Information("GitAuth: Going to membership service for user {username}", username);

                if (MembershipService.ValidateUser(username, password) == ValidationResult.Success)
                {
                    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(AuthenticationProvider.GetClaimsForUser(username)));
                    Log.Information("GitAuth: User {username} authorised by membership service", username);
                    return true;
                }
                Log.Warning("GitAuth: Membership service failed auth for {username}", username);
            }
            else
            {
                Log.Warning("GitAuth: Blank name or password {username}", username);
            }
            Log.Warning("GitAuth: User {username} not authorised", username);
            return false;
        }
    }
}