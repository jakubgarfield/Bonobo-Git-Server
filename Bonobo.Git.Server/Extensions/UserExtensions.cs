using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Serilog;

namespace Bonobo.Git.Server
{
    public static partial class UserExtensions
    {
        static string GetClaimValue(this IPrincipal user, string claimName)
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
                Log.Error(ex, "GetClaimValue exception");
            }
            return null;
        }

        public static Guid Id(this IPrincipal user)
        {
            string id = user.GetClaimValue(ClaimTypes.NameIdentifier);
            Guid result;
            if (Guid.TryParse(id, out result))
            {
                // It's a normal string Guid
                return result;
            }
            else if (String.IsNullOrEmpty(id))
            {
                // This is the anonymous user
                return Guid.Empty;
            }
            else
            {
                try
                {
                    // We might be a ADFS-style Guid is which a base64 string
                    // If this fails, we'll get a FormatException thrown anyway
                    return new Guid(Convert.FromBase64String(id));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Could not parse id '{id}' from NameIdentifier claim", id);
                    return Guid.Empty;
                }
            }
        }

        public static string Username(this IPrincipal user)
        {
            // We can tolerate the username being in either Upn or Name
            return user.GetClaimValue(ClaimTypes.Name) ?? user.GetClaimValue(ClaimTypes.Upn);
        }

        public static string DisplayName(this IPrincipal user)
        {
            return string.Format("{0} {1}", user.GetClaimValue(ClaimTypes.GivenName), user.GetClaimValue(ClaimTypes.Surname));
        }

        public static bool IsWindowsAuthenticated(this IPrincipal user)
        {
            string authenticationMethod = user.GetClaimValue(ClaimTypes.AuthenticationMethod);
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
                Log.Error(ex, "GetClaim exception");
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
                username = username.Substring(0, delimiterIndex);
            }
            delimiterIndex = username.IndexOf('\\');
            if (delimiterIndex > 0)
            {
                username = username.Substring(delimiterIndex + 1);
            }

            return username;
        }

        public static string GetDomain(this string username)
        {
            int deliIndex = username.IndexOf('@');
            if (deliIndex > 0)
            {
                return username.Substring(deliIndex + 1);
            }

            deliIndex = username.IndexOf('\\');
            if (deliIndex > 0)
            {
                return username.Substring(0, deliIndex);
            }

            return string.Empty;
        }

        // http://stackoverflow.com/questions/915745/thoughts-on-foreach-with-enumerable-range-vs-traditional-for-loop
        public static IEnumerable<int> To(this int from, int to)
        {
            if (from < to)
            {
                while (from <= to)
                {
                    yield return from++;
                }
            }
            else
            {
                while (from >= to)
                {
                    yield return from--;
                }
            }
        }

        public static IEnumerable<T> Step<T>(this IEnumerable<T> source, int step)
        {
            if (step == 0)
            {
                throw new ArgumentOutOfRangeException("step", "Param cannot be zero.");
            }

            return source.Where((x, i) => (i % step) == 0);
        }

        public static string StringlistToEscapedStringForEnvVar(IEnumerable<string> items, string separator = ",")
        {
            var y = items.Select(x => x.Replace(@"\", @"\\").Replace(separator, @"\"+separator));
            return string.Join(separator, y);
        }
    }
}