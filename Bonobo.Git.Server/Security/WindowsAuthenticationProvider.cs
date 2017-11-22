using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Bonobo.Git.Server.Security
{
    public class WindowsAuthenticationProvider : AuthenticationProvider
    {
        public WindowsAuthenticationProvider(IMembershipService membershipService, IRoleProvider roleProvider) : base(membershipService, roleProvider)
        {
        }

        public static void Configure(IServiceCollection services)
        {
            throw new NotImplementedException();
            //app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            //app.UseCookieAuthentication(new CookieAuthenticationOptions()
            //{
            //    AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
            //    LoginPath = new PathString("/Home/WindowsLogin"),
            //    Provider = new Microsoft.Owin.Security.Cookies.CookieAuthenticationProvider()
            //    {
            //        OnApplyRedirect = context =>
            //        {
            //            if (context.Request.Path != WindowsAuthenticationOptions.DefaultRedirectPath && !context.Request.Headers.ContainsKey("AuthNoRedirect"))
            //            {
            //                context.Response.Redirect(context.RedirectUri);
            //            }
            //        }
            //    }
            //});
            //app.UseWindowsAuthentication(new WindowsAuthenticationOptions
            //{
            //    GetClaimsForUser = username =>
            //    {
            //        return GetClaimsForUser(username);
            //    }
            //});
        }

        public override Task SignIn(HttpContext httpContext, string username, string returnUrl = null, bool rememberMe = false)
        {
            throw new NotImplementedException();
            //ClaimsIdentity identity = new ClaimsIdentity(GetClaimsForUser(username), WindowsAuthenticationDefaults.AuthenticationType);
            //var authprop = new AuthenticationProperties { IsPersistent = rememberMe, RedirectUri = returnUrl };
            //await httpContext.SignInAsync(authprop, identity);
            //if (!String.IsNullOrEmpty(returnUrl))
            //{
            //    httpContext.Response.Redirect(returnUrl, false);
            //}
        }

        public override Task SignOut(HttpContext httpContext)
        {
            throw new NotImplementedException();
            //return httpContext.SignOutAsync(WindowsAuthenticationDefaults.AuthenticationScheme);
        }
    }
}