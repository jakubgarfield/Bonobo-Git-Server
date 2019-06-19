using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;

namespace Bonobo.Git.Server.Security
{
    public class CookieAuthenticationProvider : AuthenticationProvider
    {
        public CookieAuthenticationProvider(IHttpContextAccessor httpContextAccessor, IMembershipService membershipService, IRoleProvider roleProvider) : base(httpContextAccessor, membershipService, roleProvider)
        {
        }

        public override void Configure(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/Home/LogOn");
                    options.ExpireTimeSpan = TimeSpan.FromDays(3);
                    options.SlidingExpiration = true;
                });
        }

        public override void SignIn(string username, string returnUrl = null, bool rememberMe = false)
        {
            ClaimsIdentity identity = new ClaimsIdentity(GetClaimsForUser(username), CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            var authprop = new AuthenticationProperties { IsPersistent = rememberMe, RedirectUri = returnUrl };
            httpContextAccessor.HttpContext.SignInAsync(principal, authprop).Wait();
            if (!String.IsNullOrEmpty(returnUrl))
            {
                httpContextAccessor.HttpContext.Response.Redirect(returnUrl, false);
            }
        }

        public override void SignOut()
        {
            httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
