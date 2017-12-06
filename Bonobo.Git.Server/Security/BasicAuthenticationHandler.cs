using System;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Bonobo.Git.Server.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace Bonobo.Git.Server.Security
{
    public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        public BasicAuthenticationHandler(IOptionsMonitor<BasicAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return Task.FromResult(HandleAuthenticate());
        }

        private AuthenticateResult HandleAuthenticate()
        {
            string authHeader = this.Context.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authHeader))
            {
                return AuthenticateResult.NoResult();
            }

            byte[] encodedDataAsBytes = Convert.FromBase64String(authHeader.Replace("Basic ", string.Empty));
            string value = Encoding.ASCII.GetString(encodedDataAsBytes);

            int colonPosition = value.IndexOf(':');
            if (colonPosition == -1)
            {
                Log.Error("GitAuth: AuthHeader doesn't contain colon - failing auth");
                return AuthenticateResult.Fail("GitAuth: AuthHeader doesn't contain colon - failing auth");
            }
            string username = value.Substring(0, colonPosition);
            string password = value.Substring(colonPosition + 1);

            Log.Verbose("GitAuth: Trying to auth user {username}", username);

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                var authenticationProvider = this.Context.RequestServices.GetService<IAuthenticationProvider>();
                var membershipService = this.Context.RequestServices.GetService<IMembershipService>();
                if (authenticationProvider is WindowsAuthenticationProvider && membershipService is EFMembershipService)
                {
                    var adHelper = this.Context.RequestServices.GetService<ADHelper>();
                    Log.Verbose("GitAuth: Going to windows auth (EF Membership) for user {username}", username);
                    if (adHelper.ValidateUser(username, password))
                    {
                        throw new NotImplementedException();
                        //httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(authenticationProvider.GetClaimsForUser(username)));
                        //Log.Verbose("GitAuth: User {username} authorised by direct windows auth", username);
                        //return true;
                    }
                }
                else
                {
                    Log.Verbose("GitAuth: Going to membership service for user {username}", username);
                    if (membershipService.ValidateUser(username, password) == ValidationResult.Success)
                    {
                        ClaimsIdentity identity = new ClaimsIdentity(authenticationProvider.GetClaimsForUser(username));
                        //identity.IsAuthenticated
                        var principal = new ClaimsPrincipal(identity);
                        Log.Verbose("GitAuth: User {username} authorised by membership service", username);

                        //var context = new ValidatePrincipalContext(this.Context, this.Scheme, this.Options, null);
                        return AuthenticateResult.Success(new AuthenticationTicket(principal, "Basic"));
                    }
                    Log.Warning("GitAuth: Membership service failed auth for {username}", username);
                    return AuthenticateResult.Fail($"GitAuth: Membership service failed auth for {username}");
                }
            }
            else
            {
                Log.Warning("GitAuth: Blank name or password {username}", username);
                return AuthenticateResult.Fail($"GitAuth: Blank name or password {username}");
            }
            Log.Warning("GitAuth: User {username} not authorized", username);
            return AuthenticateResult.Fail($"GitAuth: User {username} not authorized");
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            bool isAuth = Context.User.Identity.IsAuthenticated;
            //var realmHeader = new NameValueHeaderValue("realm", $"\"{this.Options.Realm}\"");
            this.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //this.Response.Headers.Append(HeaderNames.WWWAuthenticate, $"{Basic} {realmHeader}");
            this.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Bonobo Git\"");
            return Task.CompletedTask;
        }
    }

    public class BasicAuthenticationOptions : AuthenticationSchemeOptions
    {

    }

    public class ValidatePrincipalContext : PrincipalContext<BasicAuthenticationOptions>
    {
        public ValidatePrincipalContext(HttpContext context, AuthenticationScheme scheme, BasicAuthenticationOptions options, AuthenticationProperties properties) : base(context, scheme, options, properties)
        {
        }
    }
}
