using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Bonobo.Git.Server.Security
{
    public class CookieAuthenticationProvider : AuthenticationProvider
    {
        public CookieAuthenticationProvider(
            IMembershipService MembershipService,
            IRoleProvider RoleProvider) : base(MembershipService, RoleProvider)
        {
        }

        public static void Configure(IServiceCollection services)
        {
            AuthenticationBuilder authenticationBuilder = services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(configureOptions =>
                {
                    configureOptions.Cookie.Name = ".BonoboAuthCore";
                    configureOptions.LoginPath = new PathString("/Home/LogOn");
                    configureOptions.ExpireTimeSpan = TimeSpan.FromDays(3);
                    configureOptions.SlidingExpiration = true;
                    configureOptions.AccessDeniedPath = new PathString("/Home/Unauthorized");
                    configureOptions.Events./*OnApplyRedirect*/OnRedirectToLogin = context =>
                    {
                        if (!context.Request.Headers.ContainsKey("AuthNoRedirect"))
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }
                        return Task.CompletedTask;
                    };
                });
            AddGitAuth(authenticationBuilder);
        }

        public override async Task SignIn(HttpContext httpContext, string username, string returnUrl = null, bool rememberMe = false)
        {
            ClaimsIdentity identity = new ClaimsIdentity(GetClaimsForUser(username), CookieAuthenticationDefaults.AuthenticationScheme);
            var authprop = new AuthenticationProperties { IsPersistent = rememberMe, RedirectUri = returnUrl };
            var claimsPrincipal = new ClaimsPrincipal(identity);
            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authprop);
            if (!string.IsNullOrEmpty(returnUrl))
            {
                httpContext.Response.Redirect(returnUrl, false);
            }
        }

        public override Task SignOut(HttpContext httpContext)
        {
            return httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
