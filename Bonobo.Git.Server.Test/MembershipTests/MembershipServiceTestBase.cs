using System;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests
{
    public abstract class MembershipServiceTestBase
    {
        protected IMembershipService _service;

        [TestMethod]
        public void GetUserReturnsNullForNonExistentUser()
        {
            Assert.IsNull(_service.GetUserModel("52734589237450892374509283745092834750928347502938475"));
        }

        [TestMethod]
        public void GetUserIsCaseInsensitive()
        {
            Assert.AreEqual("admin", _service.GetUserModel("admin").Username);
            Assert.AreEqual("admin", _service.GetUserModel("ADMIN").Username);
            Assert.AreEqual("admin", _service.GetUserModel("Admin").Username);
        }

        [TestMethod]
        public void NewUserCanBeAdded()
        {
            CreateTestUser();
            Assert.AreEqual(2, _service.GetAllUsers().Count);
            var newUser = _service.GetUserModel("testuser");
            Assert.AreEqual("John", newUser.GivenName);
            Assert.AreEqual("User", newUser.Surname);
            Assert.AreEqual("test@user.com", newUser.Email);
        }

        [TestMethod]
        public void NewUserCanBeAddedWithKnownGuid()
        {
            var newUserGuid = Guid.NewGuid();
            _service.CreateUser("testUser", "hello", "John", "User", "test@user.com", newUserGuid);
            Assert.AreEqual(2, _service.GetAllUsers().Count);
            var newUser = _service.GetUserModel("testuser");
            Assert.AreEqual(newUserGuid, newUser.Id);
            Assert.AreEqual("John", newUser.GivenName);
            Assert.AreEqual("User", newUser.Surname);
            Assert.AreEqual("test@user.com", newUser.Email);
        }

        [TestMethod]
        public void UserCanBeRetrievedById()
        {
            CreateTestUser();
            var newUserByName = _service.GetUserModel("testuser");
            var newUserByGuid = _service.GetUserModel(newUserByName.Id);

            Assert.AreEqual(newUserByName.Username, newUserByGuid.Username);
            Assert.AreEqual(newUserByName.Id, newUserByGuid.Id);
        }

        [TestMethod]
        public void NewUserCanBeRetrieved()
        {
            CreateTestUser();
            var user = _service.GetUserModel("testUser");
            Assert.AreEqual("testuser", user.Username);
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
            Assert.AreEqual(2, _service.GetAllUsers().Count);
            _service.DeleteUser(_service.GetUserModel("testUser").Id);
            Assert.AreEqual(1, _service.GetAllUsers().Count);
        }


        [TestMethod]
        public void NonExistentUserDeleteIsSilentlyIgnored()
        {
            _service.DeleteUser(Guid.NewGuid());
            Assert.AreEqual(1, _service.GetAllUsers().Count);
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

        [TestMethod]
        public void UserModificationPreservesUsernameIfNull()
        {
            var userId = CreateTestUser();
            _service.UpdateUser(userId, null, "Mr", "Big", "big.admin@admin.com", "letmein");
            var newUser = _service.GetUserModel(userId);
            Assert.AreEqual("Mr", newUser.GivenName);
            Assert.AreEqual("Big", newUser.Surname);
            Assert.AreEqual("big.admin@admin.com", newUser.Email);
            Assert.AreEqual(ValidationResult.Success, _service.ValidateUser("testUser", "letmein"));
        }

        [TestMethod]
        public void UserModificationCanJustChangePassword()
        {
            var userId = CreateTestUser();
            _service.UpdateUser(userId, null, null, null, null, "newPassword");
            var newUser = _service.GetUserModel(userId);
            Assert.AreEqual("John", newUser.GivenName);
            Assert.AreEqual("User", newUser.Surname);
            Assert.AreEqual("test@user.com", newUser.Email);
            Assert.AreEqual(ValidationResult.Success, _service.ValidateUser("testUser", "newPassword"));
        }

        Guid CreateTestUser()
        {
            _service.CreateUser("testUser", "hello", "John", "User", "test@user.com");
            return _service.GetUserModel("testUser").Id;
        }
    }
}