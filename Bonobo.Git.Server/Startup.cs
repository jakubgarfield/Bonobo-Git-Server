using System;
using System.Web.Mvc;

using Microsoft.Owin;

using Owin;
using Bonobo.Git.Server.Security;

[assembly: OwinStartup(typeof(Bonobo.Git.Server.Startup))]

namespace Bonobo.Git.Server
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            DependencyResolver.Current.GetService<IAuthenticationProvider>().Configure(app);
        }
    }
}
