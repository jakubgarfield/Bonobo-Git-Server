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
            _provider = EFRoleProvider.FromCreator(() => _connection.GetContext());
            new AutomaticUpdater().RunWithContext(_connection.GetContext());
        }

        protected override BonoboGitServerContext MakeContext()
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
            _provider = EFRoleProvider.FromCreator(() => _connection.GetContext());
            new AutomaticUpdater().RunWithContext(_connection.GetContext());
        }

        protected override BonoboGitServerContext MakeContext()
        {
            return _connection.GetContext();
        }
    }

    public abstract class EFRoleProviderTest
    {
        protected IRoleProvider _provider;
        protected abstract BonoboGitServerContext MakeContext();

        [TestMethod]
        public void UpdatesCanBeRunOnAlreadyUpdatedDatabase()
        {
            // Run all the updates again - this should be completely harmless
            new AutomaticUpdater().RunWithContext(MakeContext());
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
            var fredId = AddUserFred();
            _provider.AddUserToRoles("Fred", new[] { "Programmer", "Administrator" });
            CollectionAssert.AreEqual(new[] { "Administrator", "Programmer" }, _provider.GetRolesForUser(fredId).OrderBy(role => role).ToArray());
            CollectionAssert.AreEqual(new[] { "admin", "fred" }, _provider.GetUsersInRole("Administrator").OrderBy(name => name).ToArray());
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

        Guid AddUserFred()
        {
            EFMembershipService memberService = new EFMembershipService(MakeContext);
            memberService.CreateUser("fred", "letmein", "Fred", "FredBlogs", "fred@aol", null);
            return memberService.GetUserModel("fred").Id;
        }
    }
}