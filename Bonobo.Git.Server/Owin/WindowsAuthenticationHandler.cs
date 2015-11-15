using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using System.Security.Cryptography;
using System.Text;

namespace Bonobo.Git.Server.Owin.Windows
{
    internal class WindowsAuthenticationHandler : AuthenticationHandler<WindowsAuthenticationOptions>
    {
        protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            AuthenticationProperties properties = null;
            WindowsAuthenticationHandshake handshake = null;

            string handshakeId = Request.Query["id"];

            if (handshakeId != null && Options.Handshakes.TryGet(handshakeId, out handshake))
            {
                WindowsAuthenticationToken token = WindowsAuthenticationToken.Create(Request.Headers["Authorization"]);

                switch (token.AuthorizationStage)
                {
                    case AuthenticationStage.Request:
                        if (handshake.TryAcquireServerChallenge(token))
                        {
                            Response.Headers.Add("WWW-Authenticate", new[] { string.Concat("NTLM ", token.Challenge) });
                            Response.StatusCode = 401;
                            return Task.FromResult(new AuthenticationTicket(null, properties));
                        }
                        break;
                    case AuthenticationStage.Response:
                        if (handshake.IsClientResponseValid(token))
                        {
                            properties = handshake.AuthenticationProperties;
                            if (Options.GetClaimsForUser(handshake.AuthenticatedUsername) != null)
                            {
                                Claim[] claims = Options.GetClaimsForUser(handshake.AuthenticatedUsername).ToArray();
                                if (claims.Length > 0)
                                {
                                    ClaimsIdentity identity = new ClaimsIdentity(Options.SignInAsAuthenticationType);
                                    identity.AddClaims(claims);
                                    identity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, WindowsAuthenticationDefaults.AuthenticationType));
                                    Options.Handshakes.TryRemove(handshakeId);

                                    return Task.FromResult(new AuthenticationTicket(identity, properties));
                                }
                            }
                        }
                        break;
                }

                Response.Headers.Add("WWW-Authenticate", new[] { "NTLM" });
                Response.StatusCode = 401;
            }

            return Task.FromResult(new AuthenticationTicket(null, properties));
        }

        protected override Task ApplyResponseChallengeAsync()
        {
            if (Response.StatusCode == 401 && Response.Headers.ContainsKey("WWW-Authenticate") == false)
            {
                var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

                if (challenge != null)
                {
                    AuthenticationProperties challengeProperties = challenge.Properties;

                    if (string.IsNullOrEmpty(challengeProperties.RedirectUri))
                    {
                        throw new ArgumentNullException("RedirectUri");
                    }

                    string protectedProperties = Options.StateDataFormat.Protect(challengeProperties);
                    string handshakeId = Guid.NewGuid().ToString();

                    WindowsAuthenticationHandshake handshake = new WindowsAuthenticationHandshake()
                    {
                        AuthenticationProperties = challengeProperties
                    };

                    Options.Handshakes.Add(handshakeId, handshake);
                    Response.Redirect(WebUtilities.AddQueryString(Request.PathBase + Options.CallbackPath.Value, "id", handshakeId));
                }
            }

            return Task.Delay(0);
        }

        public override async Task<bool> InvokeAsync()
        {
            bool result = false;

            if (Options.CallbackPath.HasValue && Options.CallbackPath == Request.Path)
            {
                AuthenticationTicket ticket = await AuthenticateAsync();
                if (ticket != null && ticket.Identity != null)
                {
                    Context.Authentication.SignIn(ticket.Properties, ticket.Identity);
                    Response.Redirect(ticket.Properties.RedirectUri);
                    result = true;
                }

                if (Response.Headers.ContainsKey("WWW-Authenticate"))
                {
                    result = true;
                }
            }

            return result;
        }
    }
}
