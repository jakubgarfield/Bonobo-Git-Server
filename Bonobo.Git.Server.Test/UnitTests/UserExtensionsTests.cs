using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Bonobo.Git.Server;

namespace Bonobo.Git.Server.Test.UnitTests
{
    [TestClass]
    public class HelperTests
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
    }
}
