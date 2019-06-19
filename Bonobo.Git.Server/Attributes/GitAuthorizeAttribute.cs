using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;
using Bonobo.Git.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Bonobo.Git.Server
{
    public class GitRequirement : IAuthorizationRequirement
    { }

    public class GitAuthorizationHandler : AuthorizationHandler<GitRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public IMembershipService MembershipService { get; set; }
        public IAuthenticationProvider AuthenticationProvider { get; set; }
        public IRepositoryPermissionService RepositoryPermissionService { get; set; }
        public IRepositoryRepository RepositoryRepository { get; set; }

        public GitAuthorizationHandler(IHttpContextAccessor httpContextAccessor, IMembershipService membershipService,
            IAuthenticationProvider authenticationProvider, IRepositoryPermissionService repositoryPermissionService,
            IRepositoryRepository repositoryRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            MembershipService = membershipService;
            AuthenticationProvider = authenticationProvider;
            RepositoryPermissionService = repositoryPermissionService;
            RepositoryRepository = repositoryRepository;
        }

        public static string GetRepoPath(string path, string applicationPath)
        {
            var repo = path.Replace(applicationPath, "").Replace("/","");
            return repo.Substring(0, repo.IndexOf(".git"));
        }

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
                if (AuthenticationProvider is WindowsAuthenticationProvider && MembershipService is EFMembershipService)
                {
                    Log.Verbose("GitAuth: Going to windows auth (EF Membership) for user {username}", username);
                    if (ADHelper.ValidateUser(username, password))
                    {
                        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(AuthenticationProvider.GetClaimsForUser(username)));
                        Log.Verbose("GitAuth: User {username} authorised by direct windows auth", username);
                        return true;
                    }
                }
                else
                {
                    Log.Verbose("GitAuth: Going to membership service for user {username}", username);

                    if (MembershipService.ValidateUser(username, password) == ValidationResult.Success)
                    {
                        httpContext.User =
                            new ClaimsPrincipal(new ClaimsIdentity(AuthenticationProvider.GetClaimsForUser(username)));
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

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, GitRequirement requirement)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            HttpContext httpContext = _httpContextAccessor.HttpContext;

            string incomingRepoName = GetRepoPath(httpContext.Request.Path, httpContext.Request.PathBase);
            string repoName = Repository.NormalizeRepositoryName(incomingRepoName, RepositoryRepository);

            // Add header to prevent redirection to login page even if we fail auth later (see IAuthenticationProvider.Configure)
            // If we don't fail auth later, then this is benign
            httpContext.Request.Headers.Add("AuthNoRedirect", "1");

            if (context.User != null && context.User.Identity.IsAuthenticated && context.User.Identity is ClaimsIdentity)
            {
                // We already have a claims id, we don't need to worry about the rest of these checks
                Log.Verbose("GitAuth: User {username} already has identity", httpContext.User.DisplayName());
                context.Succeed(requirement);
                return Task.FromResult(0);
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
                    context.Succeed(requirement);
                    return Task.FromResult(0);
                }
                else
                {
                    // If we're not even allowed to do an anonymous pull, then we should bounce this now, 
                    // and tell the other end to try again with an auth header included next time
                    httpContext.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Bonobo Git\"");
                    context.Fail();

                    Log.Warning("GitAuth: No auth header, anon operations not allowed");
                    return Task.FromResult(0);
                }
            }

            // Process the auth header and see if we've been given valid credentials
            if (!IsUserAuthorized(authHeader, httpContext))
            {
                context.Fail();
            }

            return Task.FromResult(0);
        }
    }
}