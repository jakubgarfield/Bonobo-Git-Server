using System;
using System.Net;
using System.Threading.Tasks;
using Bonobo.Git.Server.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Bonobo.Git.Server.Security
{
    public class FederationAuthenticationProvider : AuthenticationProvider
    {
        public FederationAuthenticationProvider(IMembershipService membershipService, IRoleProvider roleProvider) : base(membershipService, roleProvider)
        {
        }

        public static void Configure(IServiceCollection services, FederationSettings federationSettings)
        {
            if (String.IsNullOrEmpty(federationSettings.MetadataAddress))
            {
                throw new ArgumentException("Missing federation declaration in config", "FederationMetadataAddress");
            }

            if (String.IsNullOrEmpty(federationSettings.Realm))
            {
                throw new ArgumentException("Missing federation declaration in config", "FederationRealm");

            }

            var authenticationBuilder = services
                .AddAuthentication(WsFederationDefaults.AuthenticationScheme)
                .AddWsFederation(options =>
                {
                    options.Wtrealm = federationSettings.Realm;
                    options.MetadataAddress = federationSettings.Realm;
                    options.Events.OnRedirectToIdentityProvider = (context) =>
                        {
                            if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized && context.Request.Headers.ContainsKey("AuthNoRedirect"))
                            {
                                context.HandleResponse();
                            }

                            return Task.FromResult(0);
                        };
                });

            AddGitAuth(authenticationBuilder);
        }

        public override async Task SignIn(HttpContext httpContext, string username, string returnUrl, bool rememberMe)
        {
            var authprop = new AuthenticationProperties { IsPersistent = rememberMe, RedirectUri = returnUrl };
            await httpContext.ChallengeAsync(WsFederationDefaults.AuthenticationScheme, authprop);
            if (!string.IsNullOrEmpty(returnUrl))
            {
                httpContext.Response.Redirect(returnUrl, false);
            }
        }

        public override async Task SignOut(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(WsFederationDefaults.AuthenticationScheme);
        }
    }
}