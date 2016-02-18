using System;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test
{
    [TestClass]
    public class PasswordServiceTest
    {
        private const String DefaultAdminUserName = "admin";
        private const String DefaultAdminPassword = "admin";
        private const String DefaultAdminHash =
            "0CC52C6751CC92916C138D8D714F003486BF8516933815DFC11D6C3E36894BFA" +
            "044F97651E1F3EEBA26CDA928FB32DE0869F6ACFB787D5A33DACBA76D34473A3";

        private const String Md5DefaultAdminHash = "21232F297A57A5A743894A0E4A801FC3";

        [TestMethod]
        public void AdminDefaultPasswordIsSaltedSha512Hash()
        {
            Action<string, string> updateHook = (username, password) =>
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

        [TestMethod]
        public void CorrectSha512PasswordsWontBeUpgraded()
        {
            Action<string, string> updateHook = (username, password) =>
            {
                Assert.Fail("Sha512 password does not need to be upgraded.");
            };
            var passwordService = new PasswordService(updateHook);
            bool isCorrect = passwordService
                .ComparePassword(DefaultAdminPassword, DefaultAdminUserName, DefaultAdminUserName, DefaultAdminHash);
            Assert.IsTrue(isCorrect);
        }

        [TestMethod]
        public void WrongSha512PasswordsWontBeUpgraded()
        {
            Action<string, string> updateHook = (username, password) =>
            {
                Assert.Fail("Wrong sha512 password must not be upgraded.");
            };
            var passwordService = new PasswordService(updateHook);
            bool isCorrect = passwordService
                .ComparePassword("1" + DefaultAdminPassword, DefaultAdminUserName, DefaultAdminUserName, DefaultAdminHash);
            Assert.IsFalse(isCorrect);
        }

        [TestMethod]
        public void CorrectMd5PasswordsWillBeUpgraded()
        {
            int correctUpgradeHookCalls = 0;
            var username = DefaultAdminUserName;
            var password = DefaultAdminPassword;
            Action<string, string> updateHook = (updateUsername, updatePassword) =>
            {
                Assert.AreEqual(username, updateUsername);
                Assert.AreEqual(password, updatePassword);
                ++correctUpgradeHookCalls;
            };
            var passwordService = new PasswordService(updateHook);
            bool isCorrect = passwordService
                .ComparePassword(password, username, username, Md5DefaultAdminHash);
            Assert.IsTrue(isCorrect);
            Assert.AreEqual(1, correctUpgradeHookCalls, "Correct md5 password should be upgraded exactly once.");
        }

        [TestMethod]
        public void WrongMd5PasswordsWontBeUpgraded()
        {
            Action<string, string> updateHook = (s, s1) =>
            {
                Assert.Fail("Wrong md5 password must not be upgraded.");
            };
            var passwordService = new PasswordService(updateHook);
            bool isCorrect = passwordService
                .ComparePassword("1" + DefaultAdminPassword, DefaultAdminUserName, DefaultAdminUserName, Md5DefaultAdminHash);
            Assert.IsFalse(isCorrect);
        }
    }
}
