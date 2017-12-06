using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Bonobo.Git.Server.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Bonobo.Git.Server.Security
{
    public abstract class AuthenticationProvider : IAuthenticationProvider
    {
        public AuthenticationProvider(IMembershipService membershipService, IRoleProvider roleProvider)
        {
            MembershipService = membershipService;
            RoleProvider = roleProvider;
        }

        public IMembershipService MembershipService { get; set; }

        public IRoleProvider RoleProvider { get; set; }

        //public abstract void Configure(IApplicationBuilder app);
        public abstract Task SignIn(HttpContext httpContext, string username, string returnUrl, bool rememberMe);
        public abstract Task SignOut(HttpContext httpContext);

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

        protected static void AddGitAuth(AuthenticationBuilder authenticationBuilder)
        {
            authenticationBuilder.AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>("Basic", o => { });

            authenticationBuilder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Git",
                                  policy =>
                                  {
                                      //policy.AddRequirements(new GitAuthPolicy());
                                      policy.RequireClaim(ClaimTypes.NameIdentifier);
                                      policy.RequireClaim(ClaimTypes.Name);
                                      policy.RequireClaim(ClaimTypes.Email);
                                      policy.RequireClaim(ClaimTypes.Role);
                                      //policy.AddRequirements()
                                      //policy.RequireUserName("admin");
                                      policy.AddAuthenticationSchemes("Basic");
                                  });
            });


            authenticationBuilder.Services.AddSingleton<IAuthorizationHandler, GitAuthorizationHandler>();
        }
    }
}