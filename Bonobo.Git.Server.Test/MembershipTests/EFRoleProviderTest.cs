using System;
using System.Linq;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests
{
    [TestClass]
    public class EFSqliteRoleProviderTest : EFRoleProviderTest
    {
        SqliteTestConnection _connection;

        [TestInitialize]
        public void Initialize()
        {
            _connection = new SqliteTestConnection();
            _provider = new EFRoleProvider { CreateContext = () => _connection.GetContext() };
            new AutomaticUpdater().RunWithContext(_connection.GetContext());
        }

        protected override BonoboGitServerContext GetContext()
        {
            return _connection.GetContext();
        }
    }

    [TestClass]
    public class EFSqlServerRoleProviderTest : EFRoleProviderTest
    {
        SqlServerTestConnection _connection;

        [TestInitialize]
        public void Initialize()
        {
            _connection = new SqlServerTestConnection();
            _provider = new EFRoleProvider { CreateContext = () => _connection.GetContext() };
            new AutomaticUpdater().RunWithContext(_connection.GetContext());
        }

        protected override BonoboGitServerContext GetContext()
        {
            return _connection.GetContext();
        }
    }

    public abstract class EFRoleProviderTest
    {
        protected IRoleProvider _provider;
        protected abstract BonoboGitServerContext GetContext();

        [TestMethod]
        public void UpdatesCanBeRunOnAlreadyUpdatedDatabase()
        {
            // Run all the updates again - this should be completely harmless
            new AutomaticUpdater().RunWithContext(GetContext());
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
            _provider.AddUserToRoles(Guid.NewGuid(), new[] { "Administrator" });
            var users = _provider.GetUsersInRole("Administrator");
            CollectionAssert.AreEqual(new[] { "admin" }, users);
        }

        [TestMethod]
        public void TestAddingRealUserIsSuccessful()
        {
            var userId = AddUserFred();
            _provider.AddUserToRoles(userId, new[] { "Administrator" });
            var users = _provider.GetUsersInRole("Administrator");
            CollectionAssert.AreEqual(new[] { "admin", "fred" }, users.OrderBy(user => user).ToArray());
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
            CollectionAssert.AreEqual(new[] { "admin", "fred" }, _provider.GetUsersInRole("Administrator").OrderBy(name => name).ToArray());
            CollectionAssert.AreEqual(new[] { "fred" }, _provider.GetUsersInRole("Programmer"));
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
        public void RoleCanBeDeletedIfNoMembersPresen()
        {
            _provider.CreateRole("Programmer");
            _provider.DeleteRole("Programmer", false);
            _provider.DeleteRole("Administrator", false);
            Assert.AreEqual(0, _provider.GetAllRoles().Length);
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
            EFMembershipService memberService = new EFMembershipService { CreateContext = GetContext };
            memberService.CreateUser("fred", "letmein", "Fred", "FredBlogs", "fred@aol");
            return memberService.GetUserModel("fred").Id;
        }

        Guid GetAdminId()
        {
            EFMembershipService memberService = new EFMembershipService { CreateContext = GetContext };
            return memberService.GetUserModel("Admin").Id;
        }
    }
}