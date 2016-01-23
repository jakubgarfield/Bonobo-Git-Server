using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Bonobo.Git.Server
{
    public static class UserExtensions
    {
        public static string GetClaim(this IPrincipal user, string claimName)
        {
            try
            {
                ClaimsIdentity claimsIdentity = GetClaimsIdentity(user);
                if (claimsIdentity != null)
                {
                    var claim = claimsIdentity.FindFirst(claimName);
                    if (claim != null)
                    {
                        return claim.Value;
                    }
                }
            }
            catch(Exception ex)
            {
                Trace.TraceError("GetClaim Exception " + ex);
            }
            return null;
        }

        public static int Id(this IPrincipal user)
        {
            int ret = 0;
            int.TryParse(user.GetClaim(ClaimTypes.Upn), out ret);
            return ret;
        }

        public static string Username(this IPrincipal user)
        {
            return user.GetClaim(ClaimTypes.NameIdentifier);
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
            catch(Exception ex)
            {
                Trace.TraceError("GetClaim Exception " + ex);
            }

            return result;
        }

        private static ClaimsIdentity GetClaimsIdentity(this IPrincipal user)
        {
            ClaimsIdentity result = null;

            ClaimsPrincipal claimsPrincipal = user as ClaimsPrincipal;
            if (claimsPrincipal != null)
            {
                result = claimsPrincipal.Identities.FirstOrDefault(x => x != null);
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