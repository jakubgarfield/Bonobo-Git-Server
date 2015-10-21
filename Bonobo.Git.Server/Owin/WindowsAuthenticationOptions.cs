using System;
using System.Collections.Generic;
using System.Security.Claims;

using Microsoft.Owin;
using Microsoft.Owin.Security;
using System.Collections.Concurrent;

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

        public WindowsAuthenticationOptions() : base(WindowsAuthenticationDefaults.AuthenticationType)
        {
            Description.Caption = WindowsAuthenticationDefaults.AuthenticationType;
            CallbackPath = DefaultRedirectPath;
            AuthenticationMode = AuthenticationMode.Passive;
            Handshakes = new WindowsAuthenticationHandshakeCache("WindowsHandshakeCache");
        }
    }
}
