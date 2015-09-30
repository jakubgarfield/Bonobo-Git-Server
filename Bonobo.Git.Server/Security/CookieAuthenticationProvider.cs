using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

using Bonobo.Git.Server.Models;

using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Practices.Unity;

namespace Bonobo.Git.Server.Security
{
    public class CookieAuthenticationProvider : AuthenticationProvider
    {
        public override void SignIn(string username)
        {
            ClaimsIdentity identity = new ClaimsIdentity(GetClaimsForUser(username), CookieAuthenticationDefaults.AuthenticationType);
            HttpContext.Current.GetOwinContext().Authentication.SignIn(identity);
        }

        public override void SignOut()
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
        }
    }
}