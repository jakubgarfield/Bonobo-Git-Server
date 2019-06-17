using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;

namespace Bonobo.Git.Server.Owin.Windows
{
    public class WindowsAuthenticationMiddleware
    {
        //public WindowsAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, WindowsAuthenticationOptions options) : base(next, options)
        //{
        //    if (String.IsNullOrEmpty(options.SignInAsAuthenticationType))
        //    {
        //        options.SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType();
        //    }

        //    if (options.StateDataFormat == null)
        //    {
        //        IDataProtector dataProtector = app.CreateDataProtector(typeof(WindowsAuthenticationMiddleware).FullName, options.AuthenticationType);
        //        options.StateDataFormat = new PropertiesDataFormat(dataProtector);
        //    }
        //}
    }
}
