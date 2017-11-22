using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Bonobo.Git.Server.Owin.Windows
{
    public class WindowsAuthenticationOptions : AuthenticationOptions
    {
        internal static readonly PathString DefaultRedirectPath = new PathString("/windowsAuthCallback");

        public delegate IEnumerable<Claim> GetClaimsForUserDelegate(string username);

        internal WindowsAuthenticationHandshakeCache Handshakes { get; set; }
        public GetClaimsForUserDelegate GetClaimsForUser { get; set; }
        public PathString CallbackPath { get; set; }
        public string SignInAsAuthenticationType { get; set; }
        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }

        public WindowsAuthenticationOptions()
        {
            CallbackPath = DefaultRedirectPath;
            Handshakes = new WindowsAuthenticationHandshakeCache("WindowsHandshakeCache");
        }
    }
}
