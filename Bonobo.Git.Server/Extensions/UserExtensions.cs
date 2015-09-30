using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace Bonobo.Git.Server
{
    public static class UserExtensions
    {
        public static string GetClaim(this IPrincipal user, string claimName)
        {
            string result = null;

            ClaimsIdentity claimsIdentity = user.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                try
                {
                    result = claimsIdentity.FindFirst(claimName).Value;
                }
                catch
                {
                }
            }

            return result;
        }

        public static string Id(this IPrincipal user)
        {
            return user.GetClaim(ClaimTypes.Upn);
        }

        public static string Name(this IPrincipal user)
        {
            return user.GetClaim(ClaimTypes.Name);
        }

        public static string[] Roles(this IPrincipal user)
        {
            string[] result = null;

            ClaimsIdentity claimsIdentity = user.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                try
                {
                    result = claimsIdentity.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray();
                }
                catch
                {
                }
            }

            return result;
        }
    }
}