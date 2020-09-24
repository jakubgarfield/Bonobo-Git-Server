using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Security.Claims;
using System.Web;

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
                ExpireTimeSpan = TimeSpan.FromDays(3),
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

        public override void SignIn(string username, string returnUrl = null, bool rememberMe = false)
        {
            ClaimsIdentity identity = new ClaimsIdentity(GetClaimsForUser(username), CookieAuthenticationDefaults.AuthenticationType);
            var authprop = new AuthenticationProperties { IsPersistent = rememberMe, RedirectUri = returnUrl };
            HttpContext.Current.GetOwinContext().Authentication.SignIn(authprop, identity);
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
