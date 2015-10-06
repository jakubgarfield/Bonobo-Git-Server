using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

using Bonobo.Git.Server.Models;

using Microsoft.Practices.Unity;

namespace Bonobo.Git.Server.Security
{
    public abstract class AuthenticationProvider : IAuthenticationProvider
    {
        [Dependency]
        public IMembershipService MembershipService { get; set; }

        [Dependency]
        public IRoleProvider RoleProvider { get; set; }

        public abstract void SignIn(string username);
        public abstract void SignOut();

        public IEnumerable<Claim> GetClaimsForUser(string username)
        {
            UserModel user = MembershipService.GetUser(username);

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, user.DisplayName));
            claims.Add(new Claim(ClaimTypes.Upn, user.Name));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.AddRange(RoleProvider.GetRolesForUser(user.Name).Select(x => new Claim(ClaimTypes.Role, x)));

            return claims;
        }
    }
}