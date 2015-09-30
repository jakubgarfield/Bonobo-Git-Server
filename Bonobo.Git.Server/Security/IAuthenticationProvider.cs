using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace Bonobo.Git.Server.Security
{
    public interface IAuthenticationProvider
    {
        void SignIn(string username);
        void SignOut();
        IEnumerable<Claim> GetClaimsForUser(string username);
    }
}