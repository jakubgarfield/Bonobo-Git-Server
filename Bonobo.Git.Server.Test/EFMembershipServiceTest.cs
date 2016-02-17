using System.Data.Common;
using System.Linq;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test
{
    [TestClass]
    public class EFMembershipServiceTest
    {
        EFMembershipService _service;
        DbConnection _connection;

        [TestInitialize]
        public void Initialize()
        {
            _connection = DbProviderFactories.GetFactory("System.Data.SQLite").CreateConnection();
            _connection.ConnectionString = "Data Source =:memory:";
            _connection.Open();
            _service = new EFMembershipService(() => new BonoboGitServerContext(_connection));
            new AutomaticUpdater().RunWithContext(new BonoboGitServerContext(_connection));
        }

        [TestMethod]
        public void NewDatabaseContainsJustAdminUser()
        {
            var admin = _service.GetAllUsers().Single();
            Assert.AreEqual("admin", admin.Name);
        }

        [TestMethod]
        public void NewAdminUserHasCorrectPassword()
        {
            Assert.AreEqual(ValidationResult.Success, _service.ValidateUser("admin", "admin"));
        }

        [TestMethod]
        public void PasswordsAreCaseSensitive()
        {
            Assert.AreEqual(ValidationResult.Failure, _service.ValidateUser("admin", "Admin"));
        }

        [TestMethod]
        public void GetUserIsCaseInsensitive()
        {
            Assert.AreEqual("admin", _service.GetUser("admin").Name);
            Assert.AreEqual("admin", _service.GetUser("ADMIN").Name);
            Assert.AreEqual("admin", _service.GetUser("Admin").Name);
        }

        [TestMethod]
        public void NewUserCanBeAdded()
        {
            CreateTestUser();
            Assert.AreEqual(2, _service.GetAllUsers().Count);
            var newUser = _service.GetUser("testuser");
            Assert.AreEqual("Test", newUser.GivenName);
            Assert.AreEqual("User", newUser.Surname);
            Assert.AreEqual("test@user.com", newUser.Email);
        }

        [TestMethod]
        public void NewUserCanBeRetrieved()
        {
            CreateTestUser();
            var user = _service.GetUser("testUser");
            Assert.AreEqual("testuser", user.Name);
        }

        [TestMethod]
        public void NewUserCanBeDeleted()
        {
            CreateTestUser();
            Assert.AreEqual(2, _service.UserCount());
            _service.DeleteUser("testUser");
            Assert.AreEqual(1, _service.UserCount());
        }

        [TestMethod]
        public void NonExistentUserDeleteIsSilentlyIgnored()
        {
            _service.DeleteUser("testUser");
            Assert.AreEqual(1, _service.UserCount());
        }

        [TestMethod]
        public void UserCanBeModified()
        {
            _service.UpdateUser("admin", "Mr", "Big", "big.admin@admin.com", "letmein");
            var newUser = _service.GetUser("admin");
            Assert.AreEqual("Mr", newUser.GivenName);
            Assert.AreEqual("Big", newUser.Surname);
            Assert.AreEqual("big.admin@admin.com", newUser.Email);
            Assert.AreEqual(ValidationResult.Success, _service.ValidateUser("admin", "letmein"));
        }


        void CreateTestUser()
        {
            _service.CreateUser("testUser", "hello", "Test", "User", "test@user.com");
        }




    }
}