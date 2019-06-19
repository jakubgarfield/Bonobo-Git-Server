using System;
using System.Security.Claims;
using Bonobo.Git.Server.Owin.Windows;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Extensions.DependencyInjection;

namespace Bonobo.Git.Server.Security
{
    public class WindowsAuthenticationProvider : AuthenticationProvider
    {
        public WindowsAuthenticationProvider(IHttpContextAccessor httpContextAccessor, IMembershipService membershipService, IRoleProvider roleProvider) : base(httpContextAccessor, membershipService, roleProvider)
        {
        }

        public override void Configure(IServiceCollection services)
        {
            services.AddAuthentication(IISDefaults.AuthenticationScheme);
        }

        public override void SignIn(string username, string returnUrl = null, bool rememberMe = false)
        {
            ClaimsIdentity identity = new ClaimsIdentity(GetClaimsForUser(username), WindowsAuthenticationDefaults.AuthenticationType);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            var authprop = new AuthenticationProperties { IsPersistent = rememberMe, RedirectUri = returnUrl };
            httpContextAccessor.HttpContext.SignInAsync(principal, authprop);
            if (!String.IsNullOrEmpty(returnUrl))
            {
                httpContextAccessor.HttpContext.Response.Redirect(returnUrl, false);
            }
        }

        public override void SignOut()
        {
            httpContextAccessor.HttpContext.SignOutAsync(WindowsAuthenticationDefaults.AuthenticationType);
        }
    }
}