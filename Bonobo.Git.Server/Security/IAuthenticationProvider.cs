using System.Collections.Generic;
using System.Security.Claims;

using Owin;

namespace Bonobo.Git.Server.Security
{
    public interface IAuthenticationProvider
    {
        void Configure(IAppBuilder app);
        void SignIn(string username, string returnUrl);
        void SignOut();
        IEnumerable<Claim> GetClaimsForUser(string username);
    }
}