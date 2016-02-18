using System;
using System.Linq;
using System.Security.Cryptography;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test
{
    [TestClass]
    public class EFRoleProviderTest
    {
        IRoleProvider _provider;
        SqliteTestConnection _connection;

        [TestInitialize]
        public void Initialize()
        {
            _connection = new SqliteTestConnection();
            _provider = EFRoleProvider.FromCreator(() => _connection.GetContext());
            new AutomaticUpdater().RunWithContext(_connection.GetContext());
        }

        [TestMethod]
        public void UpdatesCanBeRunOnAlreadyUpdatedDatabase()
        {
            // Run all the updates again - this should be completely harmless
            new AutomaticUpdater().RunWithContext(_connection.GetContext());
        }

        [TestMethod]
        public void TestNewProviderHasJustAdminRole()
        {
            Assert.AreEqual("Administrator", _provider.GetAllRoles().Single());
        }

        [TestMethod]
        public void TestAdminRoleHasOneMember()
        {
            var users = _provider.GetUsersInRole("Administrator");
            CollectionAssert.AreEqual(new [] {"admin" }, users);
        }

        [TestMethod]
        public void TestAddingNonExistentUserToRoleIsSilentlyIgnored()
        {
            _provider.AddUserToRoles("Fred", new[] { "Administrator" });
            var users = _provider.GetUsersInRole("Administrator");
            CollectionAssert.AreEqual(new[] { "admin" }, users);
        }

        [TestMethod]
        public void TestAddingRealUserIsSuccessful()
        {
            AddUserFred();
            _provider.AddUserToRoles("Fred", new[] { "Administrator" });
            var users = _provider.GetUsersInRole("Administrator");
            CollectionAssert.AreEqual(new[] { "admin","fred" }, users);
        }

        [TestMethod]
        public void TestCreatingRole()
        {
            _provider.CreateRole("Programmer");
            CollectionAssert.AreEqual(new[] { "Administrator", "Programmer" }, _provider.GetAllRoles());
        }

        [TestMethod]
        public void TestAddingUserToMultipleRoles()
        {
            _provider.CreateRole("Programmer");
            AddUserFred();
            _provider.AddUserToRoles("Fred", new[] { "Programmer", "Administrator" });
            CollectionAssert.AreEqual(new[] { "Administrator", "Programmer" }, _provider.GetRolesForUser("fred"));
            CollectionAssert.AreEqual(new[] { "admin", "fred" }, _provider.GetUsersInRole("Administrator"));
            CollectionAssert.AreEqual(new[] { "fred" }, _provider.GetUsersInRole("Programmer"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestRoleCannotBeDeletedWhilePopulatedIfForbidden()
        {
            _provider.CreateRole("Programmer");
            _provider.AddUserToRoles("admin", new[] { "Programmer" });
            _provider.DeleteRole("Programmer", true);
        }

        [TestMethod]
        public void TestRoleCanBeDeletedWhilePopulatedIfAllowed()
        {
            _provider.CreateRole("Programmer");
            _provider.AddUserToRoles("admin", new[] { "Programmer" });
            _provider.DeleteRole("Programmer", false);
            Assert.AreEqual(1, _provider.GetAllRoles().Length);
        }

        void AddUserFred()
        {
            EFMembershipService memberService = new EFMembershipService(() => _connection.GetContext());
            memberService.CreateUser("fred", "letmein", "Fred", "FredBlogs", "fred@aol", null);
        }
    }
}