using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using Bonobo.Git.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Bonobo.Git.Server.Security
{
    public abstract class AuthenticationProvider : IAuthenticationProvider
    {
        public IMembershipService MembershipService { get; set; }
        public IRoleProvider RoleProvider { get; set; }

        protected readonly IHttpContextAccessor httpContextAccessor;

        protected AuthenticationProvider(IHttpContextAccessor httpContextAccessor, IMembershipService membershipService,
            IRoleProvider roleProvider)
        {
            this.httpContextAccessor = httpContextAccessor;
            MembershipService = membershipService;
            RoleProvider = roleProvider;
        }

        public abstract void Configure(IServiceCollection services);
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