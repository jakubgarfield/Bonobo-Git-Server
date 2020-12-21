using System;
using System.Security.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Bonobo.Git.Server.Test.Unit
{
    [TestClass]
    public class UserExtensionsTests
    {
        const string domainslashusername = @"domain.alsodomain\username";
        const string usernameatdomain = "username@domain.alsodomain";

        [TestMethod]
        public void GetDomainFromDomainSlashUsername()
        {
            Assert.AreEqual("domain.alsodomain", domainslashusername.GetDomain());
        }

        [TestMethod]
        public void StripDomainFromDomainSlashUsername()
        {
            Assert.AreEqual("username", domainslashusername.StripDomain());
        }

        [TestMethod]
        public void GetDomainFromUsernameAtDomain()
        {
            Assert.AreEqual("domain.alsodomain", usernameatdomain.GetDomain());
        }

        [TestMethod]
        public void StripDomainFromUsernameAtDomain()
        {
            Assert.AreEqual("username", usernameatdomain.StripDomain());
        }

        [TestMethod]
        public void GetGuidFromNameIdentityClaimWhenGuidStringEncoded()
        {
            var testGuid = Guid.NewGuid();
            var user = MakeUserWithClaims(new Claim(ClaimTypes.NameIdentifier, testGuid.ToString()));
            Assert.AreEqual(testGuid, user.Id());
        }

        [TestMethod]
        public void GetGuidFromNameIdentityClaimWhenGuidIsBase64Encoded()
        {
            var testGuid = Guid.NewGuid();
            var user = MakeUserWithClaims(new Claim(ClaimTypes.NameIdentifier, Convert.ToBase64String(testGuid.ToByteArray())));
            Assert.AreEqual(testGuid, user.Id());
        }

        [TestMethod]
        public void GuidIsEmptyForUserWithNoNameIdentifier()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            Assert.AreEqual(Guid.Empty, user.Id());
        }

        [TestMethod]
        public void GuidIsEmptyForUserWithUnparsableNameIdentifier()
        {
            var user = MakeUserWithClaims(new Claim(ClaimTypes.NameIdentifier, "NotAGuid"));
            Assert.AreEqual(Guid.Empty, user.Id());
        }

        [TestMethod]
        public void DisplayNameCanBeAssembledFromGivenNameAndSurname()
        {
            var user = MakeUserWithClaims(new Claim(ClaimTypes.GivenName, "Joe"), new Claim(ClaimTypes.Surname, "Bloggs"));
            Assert.AreEqual("Joe Bloggs", user.DisplayName());
        }

        [TestMethod]
        public void UsernameIsInNameClaim()
        {
            var user = MakeUserWithClaims(new Claim(ClaimTypes.Name, "JoeBloggs"));
            Assert.AreEqual("JoeBloggs", user.Username());
        }

        [TestMethod]
        public void UsernameFallsbackToUpn()
        {
            var user = MakeUserWithClaims(new Claim(ClaimTypes.Upn, "JoeBloggs@local"));
            Assert.AreEqual("JoeBloggs@local", user.Username());
        }

        [TestMethod]
        public void UsernameFallsbackToUpnOnlyIfNameIsMissing()
        {
            var user = MakeUserWithClaims(new Claim(ClaimTypes.Upn, "JoeBloggs@local"), new Claim(ClaimTypes.Name, "JoeBloggs"));
            Assert.AreEqual("JoeBloggs", user.Username());
        }

        [TestMethod]
        public void EscapeStringlistAsInFAQ()
        {
            Assert.AreEqual(@"Editors\\ Architects,Programmers\,Testers", UserExtensions.StringlistToEscapedStringForEnvVar(new List<string>{@"Editors\ Architects", "Programmers,Testers"}));
        }
        
        [TestMethod]
        public void EscapeStringlistReturnsEmptyStringforEmptyLists()
        {
            Assert.AreEqual("", UserExtensions.StringlistToEscapedStringForEnvVar(new List<string> { "" }));
            Assert.AreEqual("", UserExtensions.StringlistToEscapedStringForEnvVar(new List<string>()));
            Assert.AreEqual("", UserExtensions.StringlistToEscapedStringForEnvVar(Enumerable.Empty<string>()));
        }

        [TestMethod]
        public void EscapeStringlistWithCustomSeparatorMultiChar()
        {
            Assert.AreEqual(@"Editors\\ Architects<>Programmers\<>Testers", UserExtensions.StringlistToEscapedStringForEnvVar(new List<string>{@"Editors\ Architects", "Programmers<>Testers"}, "<>"));
        }

        [TestMethod]
        public void EscapeStringlistWithCustomSeparatorSingleChar()
        {
            Assert.AreEqual(@"Editors\\ Architects|Programmers\|Testers", UserExtensions.StringlistToEscapedStringForEnvVar(new List<string>{@"Editors\ Architects", "Programmers|Testers"}, "|"));
        }

        private static ClaimsPrincipal MakeUserWithClaims(params Claim[] claims)
        {
            var id = new ClaimsIdentity();
            foreach (var claim in claims)
            {
                id.AddClaim(claim);
            }
            return new ClaimsPrincipal(id);
        }
    }
}
