using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Bonobo.Git.Server.Configuration;

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.WsFederation;

using Owin;
using System.Net;
using System.Threading.Tasks;

namespace Bonobo.Git.Server.Security
{
    public class FederationAuthenticationProvider : AuthenticationProvider
    {
        public override void Configure(IAppBuilder app)
        {
            if (String.IsNullOrEmpty(FederationSettings.MetadataAddress))
            {
                throw new ArgumentException("Missing federation declaration in config", "FederationMetadataAddress");
            }

            if (String.IsNullOrEmpty(FederationSettings.Realm))
            {
                throw new ArgumentException("Missing federation declaration in config", "FederationRealm");

            }

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseWsFederationAuthentication(new WsFederationAuthenticationOptions
            {
                MetadataAddress = FederationSettings.MetadataAddress,
                Wtrealm = FederationSettings.Realm,
                Notifications = new WsFederationAuthenticationNotifications()
                {
                    RedirectToIdentityProvider = (context) =>
                    {
                        if (context.OwinContext.Response.StatusCode == (int)HttpStatusCode.Unauthorized && context.Request.Headers.ContainsKey("AuthNoRedirect"))
                        {
                            context.HandleResponse();
                        }

                        return Task.FromResult(0);
                    }
                }
            });
        }

        public override void SignIn(string username, string returnUrl)
        {
            HttpContext.Current.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, WsFederationAuthenticationDefaults.AuthenticationType);
        }

        public override void SignOut()
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut(WsFederationAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
        }
    }
}