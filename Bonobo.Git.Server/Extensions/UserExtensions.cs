using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Bonobo.Git.Server.Extensions
{
    public static class UserExtensions
    {
        public static string GetUsername(this IPrincipal user)
        {
            var windowsIdentity = user.Identity as WindowsIdentity;
            if (windowsIdentity == null)
            {
                return user.Identity.Name;
            }
            else
            {
                return windowsIdentity.User.Value;
            }
        }
    }
}