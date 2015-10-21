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

            try
            {
                ClaimsIdentity claimsIdentity = GetClaimsIdentity(user);
                if (claimsIdentity != null)
                {
                    result = claimsIdentity.FindFirst(claimName).Value;
                }
            }
            catch
            {
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

        public static bool IsWindowsAuthenticated(this IPrincipal user)
        {
            string authenticationMethod = user.GetClaim(ClaimTypes.AuthenticationMethod);
            return !String.IsNullOrEmpty(authenticationMethod) && authenticationMethod.Equals("Windows", StringComparison.OrdinalIgnoreCase);
        }

        public static string[] Roles(this IPrincipal user)
        {
            string[] result = null;

            try
            {
                ClaimsIdentity claimsIdentity = GetClaimsIdentity(user);
                if (claimsIdentity != null)
                {
                    result = claimsIdentity.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray();
                }
            }
            catch
            {
            }

            return result;
        }

        private static ClaimsIdentity GetClaimsIdentity(this IPrincipal user)
        {
            ClaimsIdentity result = null;

            ClaimsPrincipal claimsPrincipal = user as ClaimsPrincipal;
            if (claimsPrincipal != null)
            {
                result = claimsPrincipal.Identities.FirstOrDefault(x => x is ClaimsIdentity);
            }

            return result;
        }

        public static string StripDomain(this string username)
        {
            int delimiterIndex = username.IndexOf('@');
            if (delimiterIndex > 0)
            {
                username = username.Substring(0, delimiterIndex - 1);
            }
            delimiterIndex = username.IndexOf('\\');
            if (delimiterIndex > 0)
            {
                username = username.Substring(delimiterIndex + 1);
            }

            return username;
        }
    }
}