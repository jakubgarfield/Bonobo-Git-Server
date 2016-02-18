using System;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
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
            _connection.ConnectionString = "Data Source =:memory:;BinaryGUID=False";
            _connection.Open();
            _service = new EFMembershipService(() => new BonoboGitServerContext(_connection));
            new AutomaticUpdater().RunWithContext(new BonoboGitServerContext(_connection));
        }

        [TestMethod]
        public void UpdatesCanBeRunOnAlreadyUpdatedDatabase()
        {
            // Run all the updates again - this should be completely harmless
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
            Assert.AreEqual("admin", _service.GetUserModel("admin").Name);
            Assert.AreEqual("admin", _service.GetUserModel("ADMIN").Name);
            Assert.AreEqual("admin", _service.GetUserModel("Admin").Name);
        }

        [TestMethod]
        public void NewUserCanBeAdded()
        {
            CreateTestUser();
            Assert.AreEqual(2, _service.GetAllUsers().Count);
            var newUser = _service.GetUserModel("testuser");
            Assert.AreEqual("Test", newUser.GivenName);
            Assert.AreEqual("User", newUser.Surname);
            Assert.AreEqual("test@user.com", newUser.Email);
        }

        [TestMethod]
        public void UserCanBeRetrievedById()
        {
            CreateTestUser();
            var newUserByName = _service.GetUserModel("testuser");
            var newUserByGuid = _service.GetUserModel(newUserByName.Id);

            Assert.AreEqual(newUserByName.Name, newUserByGuid.Name);
            Assert.AreEqual(newUserByName.Id, newUserByGuid.Id);
        }

        [TestMethod]
        public void NewUserCanBeRetrieved()
        {
            CreateTestUser();
            var user = _service.GetUserModel("testUser");
            Assert.AreEqual("testuser", user.Name);
        }

        [TestMethod]
        public void NewUsersPasswordValidates()
        {
            CreateTestUser();
            Assert.AreEqual(ValidationResult.Success, _service.ValidateUser("testuseR", "hello"));
        }

        [TestMethod]
        public void NewUserCanBeDeleted()
        {
            CreateTestUser();
            Assert.AreEqual(2, _service.UserCount());
            _service.DeleteUser(_service.GetUserModel("testUser").Id);
            Assert.AreEqual(1, _service.UserCount());
        }

        [TestMethod]
        public void TestThatValidatingAUserWithDeprecatedHashUpgradesTheirPassword()
        {
            ForceInDeprecatedHash("admin", "adminpassword");
            var startingHash = GetRawUser("admin").Password;
            var startingSalt = GetPasswordSalt("admin");

            // Validation should cause the hash to be upgraded
            Assert.AreEqual(ValidationResult.Success, _service.ValidateUser("Admin", "adminpassword"));

            // We should have different salt, and different password
            Assert.AreNotEqual(startingSalt, GetPasswordSalt("admin"));
            Assert.AreNotEqual(startingHash, GetRawUser("admin").Password);
        }

        [TestMethod]
        public void TestThatFailingToValidateAUserWithDeprecatedHashDoesNotUpgradeTheirPassword()
        {
            ForceInDeprecatedHash("admin", "adminpassword");
            var startingHash = GetRawUser("admin").Password;
            var startingSalt = GetPasswordSalt("admin");

            // Validation should cause the hash to be upgraded
            Assert.AreEqual(ValidationResult.Failure, _service.ValidateUser("Admin", "adminpasswordWrong"));

            // We should have different salt, and different password
            Assert.AreEqual(startingSalt, GetPasswordSalt("admin"));
            Assert.AreEqual(startingHash, GetRawUser("admin").Password);
        }

        // This is ignored for the moment, because I haven't enabled the forced upgrade stuff
        [TestMethod, Ignore]
        public void TestThatValidatingAUserWithOldStyleSaltUpgradesTheirSalt()
        {
            // By default, the start admin user will have old-style salt (just the username)
            Assert.AreEqual("admin", GetPasswordSalt("admin"));

            Assert.AreEqual(ValidationResult.Success, _service.ValidateUser("admin", "admin"));

            // Now, the salt should have changed
            Assert.AreNotEqual("admin", GetPasswordSalt("admin"));
        }

        [TestMethod]
        public void NonExistentUserDeleteIsSilentlyIgnored()
        {
            _service.DeleteUser(Guid.NewGuid());
            Assert.AreEqual(1, _service.UserCount());
        }

        [TestMethod]
        public void UserCanBeModified()
        {
            _service.UpdateUser(_service.GetUserModel("admin").Id, "SonOfadmin", "Mr", "Big", "big.admin@admin.com", "letmein");
            var newUser = _service.GetUserModel("sonofadmiN");
            Assert.AreEqual("Mr", newUser.GivenName);
            Assert.AreEqual("Big", newUser.Surname);
            Assert.AreEqual("big.admin@admin.com", newUser.Email);
            Assert.AreEqual(ValidationResult.Success, _service.ValidateUser("sonofadmin", "letmein"));
        }

        User GetRawUser(string username)
        {
            using (var context = new BonoboGitServerContext(_connection))
            {
                username = username.ToLower();
                return context.Users.First(u => u.Username == username);
            }
        }

        string GetPasswordSalt(string username)
        {
            return GetRawUser(username).PasswordSalt;
        }

        void ForceInDeprecatedHash(string username, string password)
        {
            using (var context = new BonoboGitServerContext(_connection))
            {
                username = username.ToLower();
                var user = context.Users.First(u => u.Username == username);

                using (var hashProvider = new MD5CryptoServiceProvider())
                {
                    var data = System.Text.Encoding.UTF8.GetBytes(password);
                    data = hashProvider.ComputeHash(data);
                    user.Password = BitConverter.ToString(data).Replace("-", "");
                    user.PasswordSalt = "";
                }
                context.SaveChanges();
            }
        }

        void CreateTestUser()
        {
            _service.CreateUser("testUser", "hello", "Test", "User", "test@user.com", null);
        }
    }
}