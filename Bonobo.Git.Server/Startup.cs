using System;
using System.Net;
using System.Threading.Tasks;

using Bonobo.Git.Server.Configuration;

using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;

using Owin;

[assembly: OwinStartup(typeof(Bonobo.Git.Server.Startup))]

namespace Bonobo.Git.Server
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                LoginPath = new PathString("/Home/LogOn"),
                ExpireTimeSpan = TimeSpan.FromHours(1),
                SlidingExpiration = true,
                Provider = new CookieAuthenticationProvider
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
    }
}
