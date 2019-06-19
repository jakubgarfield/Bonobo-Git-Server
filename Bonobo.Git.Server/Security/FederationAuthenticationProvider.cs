using System;

using Bonobo.Git.Server.Configuration;

using Microsoft.AspNetCore.Authentication.WsFederation;

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Bonobo.Git.Server.Security
{
    public class FederationAuthenticationProvider : AuthenticationProvider
    {
        public FederationAuthenticationProvider(IHttpContextAccessor httpContextAccessor, IMembershipService membershipService, IRoleProvider roleProvider) : base(httpContextAccessor, membershipService, roleProvider)
        {
        }

        public override void Configure(IServiceCollection services)
        {
            if (String.IsNullOrEmpty(FederationSettings.MetadataAddress))
            {
                throw new ArgumentException("Missing federation declaration in config", "FederationMetadataAddress");
            }

            if (String.IsNullOrEmpty(FederationSettings.Realm))
            {
                throw new ArgumentException("Missing federation declaration in config", "FederationRealm");

            }

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .AddWsFederation(options =>
                {
                    options.MetadataAddress = FederationSettings.MetadataAddress;
                    options.Wtrealm = FederationSettings.Realm;
                    options.Events = new WsFederationEvents
                    {
                        OnRedirectToIdentityProvider = (context) =>
                        {
                            if (context.Response.StatusCode == (int) HttpStatusCode.Unauthorized &&
                                context.Request.Headers.ContainsKey("AuthNoRedirect"))
                            {
                                context.HandleResponse();
                            }

                            return Task.FromResult(0);
                        }
                    };
                });
        }

        public override void SignIn(string username, string returnUrl, bool rememberMe)
        {
            var authprop = new AuthenticationProperties { IsPersistent = rememberMe, RedirectUri = returnUrl };
            httpContextAccessor.HttpContext.ChallengeAsync(WsFederationDefaults.AuthenticationScheme, authprop);
            if (!String.IsNullOrEmpty(returnUrl))
            {
                httpContextAccessor.HttpContext.Response.Redirect(returnUrl, false);
            }
        }

        public override void SignOut()
        {
            httpContextAccessor.HttpContext.SignOutAsync(WsFederationDefaults.AuthenticationScheme);
            httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}