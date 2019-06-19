using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Bonobo.Git.Server.Test.Unit
{
    [TestClass]
    public class GitAuthorizeAttributeTest
    {
        [TestMethod]
        public void GetRepoPathTest()
        {
            var repo = GitAuthorizationHandler.GetRepoPath("/other/test.git/info/refs", "/other");
            Assert.AreEqual("test", repo);
            repo = GitAuthorizationHandler.GetRepoPath("/test.git/info/refs", "/");
            Assert.AreEqual("test", repo);
        }
    }
}
