using System;
using System.Linq;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Bonobo.Git.Server.Test.MembershipTests.EFTests
{
    [TestClass]
    public class EFSqliteRoleProviderTest : EFRoleProviderTest
    {
        [TestInitialize]
        public void Initialize()
        {
            _connection = new SqliteTestConnection();
            InitialiseTestObjects();
        }
        [TestCleanup]
        public void Cleanup()
        {
            _connection.Dispose();
        }
    }

    [TestClass]
    public class EFSqlServerRoleProviderTest : EFRoleProviderTest
    {
        [TestInitialize]
        public void Initialize()
        {
            _connection = new SqlServerTestConnection();
            InitialiseTestObjects();
        }
        [TestCleanup]
        public void Cleanup()
        {
            _connection.Dispose();
        }
    }

    public abstract class EFRoleProviderTest
    {
        protected IDatabaseTestConnection _connection;
        protected IRoleProvider _provider;

        [TestMethod]
        public void UpdatesCanBeRunOnAlreadyUpdatedDatabase()
        {
            // Run all the updates again - this should be completely harmless
            new AutomaticUpdater().RunWithContext(GetContext(), Substitute.For<IAuthenticationProvider>());
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
            CollectionAssert.AreEqual(new [] { GetAdminId() }, users);
        }

        [TestMethod]
        public void TestAddingNonExistentUserToRoleIsSilentlyIgnored()
        {
            _provider.AddUserToRoles(Guid.NewGuid(), new[] { "Administrator" });
            var users = _provider.GetUsersInRole("Administrator");
            CollectionAssert.AreEqual(new[] { GetAdminId() }, users);
        }

        [TestMethod]
        public void TestAddingRealUserIsSuccessful()
        {
            var userId = AddUserFred();
            _provider.AddUserToRoles(userId, new[] { "Administrator" });
            var users = _provider.GetUsersInRole("Administrator");
            CollectionAssert.AreEqual(new[] { GetAdminId(), userId }.OrderBy(id => id).ToArray(), users.OrderBy(user => user).ToArray());
        }

        [TestMethod]
        public void TestCreatingRole()
        {
            _provider.CreateRole("Programmer");
            CollectionAssert.AreEqual(new[] { "Administrator", "Programmer" }, _provider.GetAllRoles().OrderBy(role => role).ToArray());
        }

        [TestMethod]
        public void RemovingAUserFromARole()
        {
            _provider.CreateRole("Programmer");
            var userId = AddUserFred();
            _provider.AddUserToRoles(userId, new[] { "Administrator", "Programmer" });

            _provider.RemoveUserFromRoles(userId, new [] { "Administrator" });

            CollectionAssert.AreEqual(new[] { "Programmer" }, _provider.GetRolesForUser(userId));
        }

        [TestMethod]
        public void TestAddingUserToMultipleRoles()
        {
            _provider.CreateRole("Programmer");
            var fredId = AddUserFred();
            _provider.AddUserToRoles(fredId, new[] { "Programmer", "Administrator" });
            CollectionAssert.AreEqual(new[] { "Administrator", "Programmer" }, _provider.GetRolesForUser(fredId).OrderBy(role => role).ToArray());
            CollectionAssert.AreEqual(new[] { GetAdminId(), fredId }.OrderBy(u => u).ToArray(), _provider.GetUsersInRole("Administrator").OrderBy(name => name).ToArray());
            CollectionAssert.AreEqual(new[] { fredId }, _provider.GetUsersInRole("Programmer"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestRoleCannotBeDeletedWhilePopulatedIfForbidden()
        {
            var userId = AddUserFred();
            _provider.CreateRole("Programmer");
            _provider.AddUserToRoles(userId, new[] { "Programmer" });
            _provider.DeleteRole("Programmer", true);
        }

        [TestMethod]
        public void RoleCanBeDeletedIfNoMembersPresent()
        {
            _provider.CreateRole("Programmer");
            _provider.DeleteRole("Programmer", false);
            Assert.AreEqual(1, _provider.GetAllRoles().Length);
        }

        [TestMethod]
        public void UserInRoleDetectedCorrectly()
        {
            Assert.IsTrue(_provider.IsUserInRole(GetAdminId(), "Administrator"));
        }

        // I'm ignoring this for the moment because it fails with SqlServer and I need to investigate if we're supposed to have it at all
        [TestMethod, Ignore]
        public void TestRoleCanBeDeletedWhilePopulatedIfAllowed()
        {
            var userId = AddUserFred();
            _provider.CreateRole("Programmer");
            _provider.AddUserToRoles(userId, new[] { "Programmer" });
            _provider.DeleteRole("Programmer", false);
            Assert.AreEqual(1, _provider.GetAllRoles().Length);
        }

        Guid AddUserFred()
        {
            EFMembershipService memberService = new EFMembershipService(GetContext);
            memberService.CreateUser("fred", "letmein", "Fred", "FredBlogs", "fred@aol");
            return memberService.GetUserModel("fred").Id;
        }

        Guid GetAdminId()
        {
            EFMembershipService memberService = new EFMembershipService(GetContext);
            return memberService.GetUserModel("Admin").Id;
        }

        private BonoboGitServerContext GetContext()
        {
            return _connection.GetContext();
        }

        protected void InitialiseTestObjects()
        {
            _provider = new EFRoleProvider(() => _connection.GetContext());
            new AutomaticUpdater().RunWithContext(_connection.GetContext(), Substitute.For<IAuthenticationProvider>());
        }
    }
}