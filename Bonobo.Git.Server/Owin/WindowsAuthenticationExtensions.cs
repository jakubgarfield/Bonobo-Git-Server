using System;
using Microsoft.AspNetCore.Authentication;

namespace Bonobo.Git.Server.Owin.Windows
{
    public static class WindowsAuthenticationExtensions
    {
        public static AuthenticationBuilder AddWindows(this AuthenticationBuilder builder, Action<WindowsAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<WindowsAuthenticationOptions, WindowsAuthenticationHandler>(
                WindowsAuthenticationDefaults.AuthenticationType, configureOptions);
        }
    }
}