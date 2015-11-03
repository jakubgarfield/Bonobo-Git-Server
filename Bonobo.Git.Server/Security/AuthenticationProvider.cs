using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

using Bonobo.Git.Server.Models;

using Microsoft.Practices.Unity;

using Owin;

namespace Bonobo.Git.Server.Security
{
    public abstract class AuthenticationProvider : IAuthenticationProvider
    {
        [Dependency]
        public IMembershipService MembershipService { get; set; }

        [Dependency]
        public IRoleProvider RoleProvider { get; set; }

        public abstract void Configure(IAppBuilder app);
        public abstract void SignIn(string username, string returnUrl);
        public abstract void SignOut();

        public IEnumerable<Claim> GetClaimsForUser(string username)
        {
            List<Claim> result = null;

            UserModel user = MembershipService.GetUser(username);
            if (user != null)
            {
                result = new List<Claim>();
                result.Add(new Claim(ClaimTypes.Name, user.DisplayName));
                result.Add(new Claim(ClaimTypes.Upn, user.Name));
                result.Add(new Claim(ClaimTypes.Email, user.Email));
                result.Add(new Claim(ClaimTypes.Role, Definitions.Roles.Member));
                result.AddRange(RoleProvider.GetRolesForUser(user.Name).Select(x => new Claim(ClaimTypes.Role, x)));
            }

            return result;
        }
    }
}