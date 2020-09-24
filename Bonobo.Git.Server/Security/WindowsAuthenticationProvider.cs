using Bonobo.Git.Server.Owin.Windows;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Security.Claims;
using System.Web;

namespace Bonobo.Git.Server.Security
{
    public class WindowsAuthenticationProvider : AuthenticationProvider
    {
        public override void Configure(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                LoginPath = new PathString("/Home/WindowsLogin"),
                Provider = new Microsoft.Owin.Security.Cookies.CookieAuthenticationProvider()
                {
                    OnApplyRedirect = context =>
                    {
                        if (context.Request.Path != WindowsAuthenticationOptions.DefaultRedirectPath && !context.Request.Headers.ContainsKey("AuthNoRedirect"))
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }
                    }
                }
            });
            app.UseWindowsAuthentication(new WindowsAuthenticationOptions
            {
                GetClaimsForUser = username =>
                {
                    return GetClaimsForUser(username);
                }
            });
        }

        public override void SignIn(string username, string returnUrl = null, bool rememberMe = false)
        {
            ClaimsIdentity identity = new ClaimsIdentity(GetClaimsForUser(username), WindowsAuthenticationDefaults.AuthenticationType);
            var authprop = new AuthenticationProperties { IsPersistent = rememberMe, RedirectUri = returnUrl };
            HttpContext.Current.GetOwinContext().Authentication.SignIn(authprop, identity);
            if (!String.IsNullOrEmpty(returnUrl))
            {
                HttpContext.Current.Response.Redirect(returnUrl, false);
            }
        }

        public override void SignOut()
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut(WindowsAuthenticationDefaults.AuthenticationType);
        }
    }
}