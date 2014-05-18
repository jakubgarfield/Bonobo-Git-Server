using System;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test
{
    [TestClass]
    public class PasswordHashTest
    {
        private const String DefaultAdminUserName = "admin";
        private const String DefaultAdminPassword = "admin";
        private const String DefaultAdminHash = 
            "0CC52C6751CC92916C138D8D714F003486BF8516933815DFC11D6C3E36894BFA"+
            "044F97651E1F3EEBA26CDA928FB32DE0869F6ACFB787D5A33DACBA76D34473A3";

        [TestMethod]
        public void AdminDefaultPasswordIsSaltedSha512Hash()
        {
            Action<string, string> updateHook = (s, s1) =>
            {
                Assert.Fail("Generating password hash should not update the related db entry.");
            };
            var passwordService = new PasswordService(updateHook);
            var saltedHash = passwordService.GetSaltedHash(DefaultAdminPassword, DefaultAdminUserName);
            Assert.AreEqual(DefaultAdminHash, saltedHash);
        }

        [TestMethod]
        public void InsertDefaultDataCommandUsesSaltedSha512Hash()
        {
            var script = new Bonobo.Git.Server.Data.Update.InsertDefaultData();
            AssertSaltedSha512HashIsUsed(script);
        }

        [TestMethod]
        public void SqlServerInsertDefaultDataCommandUsesSaltedSha512Hash()
        {
            var script = new Bonobo.Git.Server.Data.Update.SqlServer.InsertDefaultData();
            AssertSaltedSha512HashIsUsed(script);
        }

        // ReSharper disable UnusedParameter.Global
        public void AssertSaltedSha512HashIsUsed(IUpdateScript updateScript)
        // ReSharper restore UnusedParameter.Global
        {
            Assert.IsTrue(updateScript.Command.Contains(DefaultAdminHash));
        }

        // todo: test backwards compatibility
    }
}
