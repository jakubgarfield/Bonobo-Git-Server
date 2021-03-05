using Bonobo.Git.Server.Models;
using Owin;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Unity;

namespace Bonobo.Git.Server.Security
{
    public abstract class AuthenticationProvider : IAuthenticationProvider
    {
        [Dependency]
        public IMembershipService MembershipService { get; set; }

        [Dependency]
        public IRoleProvider RoleProvider { get; set; }

        public abstract void Configure(IAppBuilder app);
        public abstract void SignIn(string username, string returnUrl, bool rememberMe);
        public abstract void SignOut();

        public IEnumerable<Claim> GetClaimsForUser(string username)
        {
            List<Claim> result = null;

            UserModel user = MembershipService.GetUserModel(username);
            if (user != null)
            {
                result = new List<Claim>();
                result.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                result.Add(new Claim(ClaimTypes.Name, user.Username));
                result.Add(new Claim(ClaimTypes.GivenName, user.GivenName));
                result.Add(new Claim(ClaimTypes.Surname, user.Surname));
                result.Add(new Claim(ClaimTypes.Email, user.Email));
                result.Add(new Claim(ClaimTypes.Role, Definitions.Roles.Member));
                result.AddRange(RoleProvider.GetRolesForUser(user.Id).Select(x => new Claim(ClaimTypes.Role, x)));
            }

            return result;
        }
    }
}