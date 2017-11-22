//using System;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using System.Collections.Generic;

//using Microsoft.Owin.Infrastructure;
//using Microsoft.Owin.Security;
//using Microsoft.Owin.Security.Infrastructure;
//using System.Security.Cryptography;
//using System.Text;
//using System.DirectoryServices.AccountManagement;

//using Bonobo.Git.Server.Security;
//using Bonobo.Git.Server.Helpers;
//using Serilog;

//namespace Bonobo.Git.Server.Owin.Windows
//{
//    internal class WindowsAuthenticationHandler : AuthenticationHandler<WindowsAuthenticationOptions>
//    {
//        protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
//        {
//            AuthenticationProperties properties = null;
//            WindowsAuthenticationHandshake handshake = null;

//            string handshakeId = Request.Query["id"];

//            if (handshakeId != null && Options.Handshakes.TryGet(handshakeId, out handshake))
//            {
//                WindowsAuthenticationToken token = WindowsAuthenticationToken.Create(Request.Headers["Authorization"]);

//                switch (token.AuthorizationStage)
//                {
//                    case AuthenticationStage.Request:
//                        if (handshake.TryAcquireServerChallenge(token))
//                        {
//                            Log.Verbose("WinAuth: Obtained challenge token OK");

//                            Response.Headers.Add("WWW-Authenticate", new[] { string.Concat("NTLM ", token.Challenge) });
//                            Response.StatusCode = 401;
//                            return Task.FromResult(new AuthenticationTicket(null, properties));
//                        }
//                        break;
//                    case AuthenticationStage.Response:
//                        if (handshake.IsClientResponseValid(token))
//                        {
//                            properties = handshake.AuthenticationProperties;
//                            var uid = handshake.AuthenticatedUsername.ToLowerInvariant();
//                            var claimdelegate = Options.GetClaimsForUser(uid);

//                            Log.Verbose("WinAuth: Valid response for uid {UserId}", uid);

//                            if (claimdelegate == null) 
//                            {
//                                string domainName = handshake.AuthenticatedUsername.GetDomain();

//                                Log.Verbose("WinAuth: New user - looking-up user {UserName} in domain {DomainName}",
//                                    handshake.AuthenticatedUsername, domainName);

//                                var dc = new PrincipalContext(ContextType.Domain, domainName);
//                                var adUser = UserPrincipal.FindByIdentity(dc, handshake.AuthenticatedUsername);

//                                if (adUser == null)
//                                {
//                                    Log.Error("DC for domain {DomainName} has returned null for username {UserName} - failing auth", domainName, handshake.AuthenticatedUsername);
//                                    Response.StatusCode = 401;
//                                    return Task.FromResult(new AuthenticationTicket(null, null));
//                                }

//                                Log.Verbose("WinAuth: DC returned adUser {ADUser}", adUser.GivenName);

//                                ClaimsIdentity identity = new ClaimsIdentity(Options.SignInAsAuthenticationType);
//                                List<Claim> result = new List<Claim>();
//                                if (!String.IsNullOrEmpty(adUser.GivenName))
//                                {
//                                    result.Add(new Claim(ClaimTypes.GivenName, adUser.GivenName));
//                                }
//                                if (!String.IsNullOrEmpty(adUser.Surname))
//                                {
//                                    result.Add(new Claim(ClaimTypes.Surname, adUser.Surname));
//                                }
//                                result.Add(new Claim(ClaimTypes.NameIdentifier, adUser.Guid.ToString()));
//                                result.Add(new Claim(ClaimTypes.Name, handshake.AuthenticatedUsername));
//                                if (!String.IsNullOrEmpty(adUser.EmailAddress))
//                                {
//                                    result.Add(new Claim(ClaimTypes.Email, adUser.EmailAddress));
//                                }
//                                identity.AddClaims(result);
//                                identity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, WindowsAuthenticationDefaults.AuthenticationType));
//                                Options.Handshakes.TryRemove(handshakeId);

//                                Log.Verbose("WinAuth: New user - about to redirect to CreateADUser");

//                                // user does not exist! Redirect to create page.
//                                properties.RedirectUri = "/Account/CreateADUser";
//                                return Task.FromResult(new AuthenticationTicket(identity, properties));
//                            }
//                            else
//                            {
//                                Claim[] claims = claimdelegate.ToArray();
//                                Log.Verbose("WinAuth: Found existing uid {UserId}, has {Claims} claims", uid, claims.Length);
//                                if (claims.Length > 0)
//                                {
//                                    ClaimsIdentity identity = new ClaimsIdentity(Options.SignInAsAuthenticationType);
//                                    identity.AddClaims(claims);
//                                    identity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, WindowsAuthenticationDefaults.AuthenticationType));
//                                    Options.Handshakes.TryRemove(handshakeId);

//                                    Log.Verbose("WinAuth: Returning id auth ticket, claims: {Claims}", claims);

//                                    return Task.FromResult(new AuthenticationTicket(identity, properties));
//                                }
//                            }
//                        }
//                        break;
//                }
//                Response.Headers.Add("WWW-Authenticate", new[] { "NTLM" });
//                Response.StatusCode = 401;
//            }
//            return Task.FromResult(new AuthenticationTicket(null, properties));
//        }

//        protected override Task ApplyResponseChallengeAsync()
//        {
//            if (Response.StatusCode == 401 && Response.Headers.ContainsKey("WWW-Authenticate") == false)
//            {
//                var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

//                if (challenge != null)
//                {
//                    AuthenticationProperties challengeProperties = challenge.Properties;

//                    if (string.IsNullOrEmpty(challengeProperties.RedirectUri))
//                    {
//                        throw new ArgumentNullException("RedirectUri");
//                    }

//                    string protectedProperties = Options.StateDataFormat.Protect(challengeProperties);
//                    string handshakeId = Guid.NewGuid().ToString();

//                    WindowsAuthenticationHandshake handshake = new WindowsAuthenticationHandshake()
//                    {
//                        AuthenticationProperties = challengeProperties
//                    };

//                    Options.Handshakes.Add(handshakeId, handshake);
//                    Response.Redirect(WebUtilities.AddQueryString(Request.PathBase + Options.CallbackPath.Value, "id", handshakeId));
//                }
//            }

//            return Task.Delay(0);
//        }

//        public override async Task<bool> InvokeAsync()
//        {
//            bool result = false;

//            if (Options.CallbackPath.HasValue && Options.CallbackPath == Request.Path)
//            {
//                AuthenticationTicket ticket = await AuthenticateAsync();
//                if (ticket != null && ticket.Identity != null)
//                {

//                    Context.Authentication.SignIn(ticket.Properties, ticket.Identity);
//                    if(!ticket.Properties.RedirectUri.StartsWith(Request.PathBase.Value))
//                    {
//                        ticket.Properties.RedirectUri = Request.PathBase.Value + ticket.Properties.RedirectUri;
//                    }
//                    Response.Redirect(ticket.Properties.RedirectUri);
//                    result = true;
//                }

//                if (Response.Headers.ContainsKey("WWW-Authenticate"))
//                {
//                    result = true;
//                }
//            }

//            return result;
//        }
//    }
//}
