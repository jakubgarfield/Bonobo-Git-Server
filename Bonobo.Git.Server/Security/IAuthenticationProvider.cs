using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

namespace Bonobo.Git.Server.Security
{
    public interface IAuthenticationProvider
    {
        void Configure(IServiceCollection services);
        void SignIn(string username, string returnUrl, bool rememberMe);
        void SignOut();
        IEnumerable<Claim> GetClaimsForUser(string username);
    }
}