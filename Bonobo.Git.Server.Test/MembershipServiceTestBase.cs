using System;
using System.Linq;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test
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

        void CreateTestUser()
        {
            _service.CreateUser("testUser", "hello", "Test", "User", "test@user.com", null);
        }
    }
}