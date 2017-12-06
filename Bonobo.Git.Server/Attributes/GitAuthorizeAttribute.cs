using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Helpers;
using Bonobo.Git.Server.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bonobo.Git.Server
{

    public class GitAuthPolicy : IAuthorizationRequirement
    {
        public GitAuthPolicy()
        {
        }
    }

    public class GitAuthorizationHandler : AuthorizationHandler<GitAuthPolicy>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, GitAuthPolicy requirement)
        {

            if (context.Resource is Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext mvcContext)
            {
                var httpContext = mvcContext.HttpContext;

                string incomingRepoName = GetRepoPath(httpContext.Request.Path, /*httpContext.Request.ApplicationPath*/ "/");
                var repositoryRepository = httpContext.RequestServices.GetService<IRepositoryRepository>();
                string repoName = Repository.NormalizeRepositoryName(incomingRepoName, repositoryRepository);

                var authenticationProvider = httpContext.RequestServices.GetService<IAuthenticationProvider>();
                var membershipService = httpContext.RequestServices.GetService<IMembershipService>();

                //if (context.User.Identity.IsAuthenticated)
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }

        public static string GetRepoPath(string path, string applicationPath)
        {
            var repo = path.Replace(applicationPath, "").Replace("/", "");
            return repo.Substring(0, repo.IndexOf(".git"));
        }
    }


    public class GitAuthorizeAttribute : AuthorizeAttribute//, IAuthorizationFilter
    {
        public static string GetRepoPath(string path, string applicationPath)
        {
            var repo = path.Replace(applicationPath, "").Replace("/", "");
            return repo.Substring(0, repo.IndexOf(".git"));
        }

        //public void OnAuthorization(AuthorizationFilterContext context)
        //{
        //    if (context == null)
        //    {
        //        throw new ArgumentNullException(nameof(context));
        //    }

        //    var httpContext = context.HttpContext;

        //    string incomingRepoName = GetRepoPath(httpContext.Request.Path, /*httpContext.Request.ApplicationPath*/ "/");
        //    var repositoryRepository = context.HttpContext.RequestServices.GetService<IRepositoryRepository>();
        //    string repoName = Repository.NormalizeRepositoryName(incomingRepoName, repositoryRepository);

        //    // Add header to prevent redirection to login page even if we fail auth later (see IAuthenticationProvider.Configure)
        //    // If we don't fail auth later, then this is benign
        //    httpContext.Request.Headers.Add("AuthNoRedirect", "1");

        //    if (httpContext.User.Identity.IsAuthenticated && httpContext.User != null && httpContext.User.Identity is System.Security.Claims.ClaimsIdentity)
        //    {
        //        // We already have a claims id, we don't need to worry about the rest of these checks
        //        Log.Verbose("GitAuth: User {username} already has identity", httpContext.User.DisplayName());
        //        return;
        //    }

        //    string authHeader = httpContext.Request.Headers["Authorization"];

        //    if (String.IsNullOrEmpty(authHeader))
        //    {
        //        // We don't have an auth header, but if we're doing an anonymous operation, that's OK
        //        var repositoryPermissionService = context.HttpContext.RequestServices.GetService<IRepositoryPermissionService>();
        //        if (repositoryPermissionService.HasPermission(Guid.Empty, repoName, RepositoryAccessLevel.Pull))
        //        {
        //            // Allow this through.  If it turns out they're actually trying to do an anon push and that's not allowed for this repo
        //            // then the GitController will reject them in there
        //            Log.Information("GitAuth: No auth header, anon operation may be allowed");
        //            return;
        //        }
        //        else
        //        {
        //            // If we're not even allowed to do an anonymous pull, then we should bounce this now, 
        //            // and tell the other end to try again with an auth header included next time
        //            //httpContext.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Bonobo Git\"");
        //            context.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);

        //            Log.Warning("GitAuth: No auth header, anon operations not allowed");
        //            return;
        //        }
        //    }

        //    // Process the auth header and see if we've been given valid credentials
        //    if (!IsUserAuthorized(authHeader, httpContext))
        //    {
        //        context.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);
        //    }
        //}

        private bool IsUserAuthorized(string authHeader, HttpContext httpContext)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(authHeader.Replace("Basic ", String.Empty));
            string value = Encoding.ASCII.GetString(encodedDataAsBytes);

            int colonPosition = value.IndexOf(':');
            if (colonPosition == -1)
            {
                Log.Error("GitAuth: AuthHeader doesn't contain colon - failing auth");
                return false;
            }
            string username = value.Substring(0, colonPosition);
            string password = value.Substring(colonPosition + 1);

            Log.Verbose("GitAuth: Trying to auth user {username}", username);

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                var authenticationProvider = httpContext.RequestServices.GetService<IAuthenticationProvider>();
                var membershipService = httpContext.RequestServices.GetService<IMembershipService>();
                if (authenticationProvider is WindowsAuthenticationProvider && membershipService is EFMembershipService)
                {
                    var adHelper = httpContext.RequestServices.GetService<ADHelper>();
                    Log.Verbose("GitAuth: Going to windows auth (EF Membership) for user {username}", username);
                    if (adHelper.ValidateUser(username, password))
                    {
                        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(authenticationProvider.GetClaimsForUser(username)));
                        Log.Verbose("GitAuth: User {username} authorised by direct windows auth", username);
                        return true;
                    }
                }
                else
                {
                    Log.Verbose("GitAuth: Going to membership service for user {username}", username);
                    if (membershipService.ValidateUser(username, password) == ValidationResult.Success)
                    {
                        ClaimsIdentity identity = new ClaimsIdentity(authenticationProvider.GetClaimsForUser(username));
                        //identity.IsAuthenticated
                        httpContext.User = new ClaimsPrincipal(identity);
                        Log.Verbose("GitAuth: User {username} authorised by membership service", username);
                        return true;
                    }
                    Log.Warning("GitAuth: Membership service failed auth for {username}", username);
                }
            }
            else
            {
                Log.Warning("GitAuth: Blank name or password {username}", username);
            }
            Log.Warning("GitAuth: User {username} not authorized", username);
            return false;
        }
    }
}