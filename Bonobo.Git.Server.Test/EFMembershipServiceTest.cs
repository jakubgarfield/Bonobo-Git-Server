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
            _service.CreateUser("testUser", "hello", "Test", "User", "test@user.com");
            Assert.AreEqual(2, _service.GetAllUsers().Count);
        }

        [TestMethod]
        public void NewUserCanBeRetrieved()
        {
            _service.CreateUser("testUser", "hello", "Test", "User", "test@user.com");
            var user = _service.GetUser("testUser");
            Assert.AreEqual("testuser", user.Name);
        }


    }
}