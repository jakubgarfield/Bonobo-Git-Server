using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Bonobo.Git.Server.Security
{
    public interface IAuthenticationProvider
    {
        Task SignIn(HttpContext httpContext, string username, string returnUrl, bool rememberMe);
        Task SignOut(HttpContext httpContext);
        IEnumerable<Claim> GetClaimsForUser(string username);
    }
}