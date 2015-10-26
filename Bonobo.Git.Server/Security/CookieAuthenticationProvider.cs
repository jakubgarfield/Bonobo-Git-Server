using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

using Bonobo.Git.Server.Models;

using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Practices.Unity;

using Owin;

namespace Bonobo.Git.Server.Security
{
    public class CookieAuthenticationProvider : AuthenticationProvider
    {
        public override void Configure(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                LoginPath = new PathString("/Home/LogOn"),
                ExpireTimeSpan = TimeSpan.FromHours(1),
                SlidingExpiration = true,
                Provider = new Microsoft.Owin.Security.Cookies.CookieAuthenticationProvider
                {
                    OnApplyRedirect = context =>
                    {
                        if (!context.Request.Headers.ContainsKey("AuthNoRedirect"))
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }
                    }
                },
            });
        }

        public override void SignIn(string username, string returnUrl = null)
        {
            ClaimsIdentity identity = new ClaimsIdentity(GetClaimsForUser(username), CookieAuthenticationDefaults.AuthenticationType);
            HttpContext.Current.GetOwinContext().Authentication.SignIn(identity);
            if (!String.IsNullOrEmpty(returnUrl))
            {
                HttpContext.Current.Response.Redirect(returnUrl, false);
            }
        }

        public override void SignOut()
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
        }
    }
}
