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
            var repo = GitAuthorizeAttribute.GetRepoPath("/other/test.git/info/refs", "/other", false);
            Assert.AreEqual(@"\test.git", repo);
            repo = GitAuthorizeAttribute.GetRepoPath("/test.git/info/refs", "/", false);
            Assert.AreEqual("test.git", repo);
        }
    }
}
