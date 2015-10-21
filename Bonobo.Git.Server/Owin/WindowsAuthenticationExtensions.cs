using Microsoft.Owin.Extensions;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Owin.Windows
{
    public static class WindowsAuthenticationExtensions
    {
        public static IAppBuilder UseWindowsAuthentication(this IAppBuilder app, WindowsAuthenticationOptions options)
        {
            app.Use(typeof(WindowsAuthenticationMiddleware), app, options);
            return app.UseStageMarker(PipelineStage.Authenticate);
        }
    }
}