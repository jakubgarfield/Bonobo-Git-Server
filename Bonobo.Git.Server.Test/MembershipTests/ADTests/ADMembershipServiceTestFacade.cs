using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests.ADTests
{
    /// <summary>
    ///  This is a wrapper on the ADMembershipService which implements various functions
    ///  that you can't normally do with the ADMembershipService, thereby allowing you to test the rest of it
    /// </summary>
    internal class ADMembershipServiceTestFacade : IMembershipService
    {
        private readonly ADMembershipService _service;
        private readonly ADTestSupport _testSupport;
        private readonly Dictionary<Guid, string> _passwords = new Dictionary<Guid, string>();

        public ADMembershipServiceTestFacade(ADMembershipService service, ADTestSupport testSupport)
        {
            _service = service;
            _testSupport = testSupport;

            ADBackend.Instance.Users.Add(new UserModel() { Username = "admin", Id = Guid.NewGuid() });
            ADBackend.Instance.Roles.Add(new RoleModel() { Name  = "Administrator", Id = Guid.NewGuid(), Members = new[] { "admin"}});
            Assert.AreEqual(1, ADBackend.Instance.Users.Count());
        }

        public bool IsReadOnly()
        {
            return _service.IsReadOnly();
        }

        public ValidationResult ValidateUser(string username, string password)
        {
            // We can't do this without a real back end service
            return _passwords[GetUserModel(username).Id] == password ? ValidationResult.Success : ValidationResult.Failure;
        }

        public bool CreateUser(string username, string password, string givenName, string surname, string email, Guid id)
        {
            _passwords[id] = password;
            return _testSupport.CreateUser(username, password, givenName, surname, email, id) != null;
        }

        public bool CreateUser(string username, string password, string givenName, string surname, string email)
        {
            return CreateUser(username, password, givenName, surname, email, Guid.NewGuid());
        }

        public IList<UserModel> GetAllUsers()
        {
            return _service.GetAllUsers();
        }

        public UserModel GetUserModel(string username)
        {
            username = username.ToLowerInvariant();
            return ADBackend.Instance.Users.FirstOrDefault(n => n.Username == username);
        }

        public UserModel GetUserModel(Guid id)
        {
            return _service.GetUserModel(id);
        }

        public void UpdateUser(Guid id, string username, string givenName, string surname, string email, string password)
        {
            var model = GetUserModel(id);
            if (username != null)
            {
                model.Username = username.ToLowerInvariant();
            }
            if (givenName != null)
            {
                model.GivenName = givenName;
            }
            if (surname != null)
            {
                model.Surname = surname;
            }
            if (email != null)
            {
                model.Email = email;
            }
            if (password != null)
            {
                _passwords[id] = password;
            }
            ADBackend.Instance.Users.Update(model);
        }

        public void DeleteUser(Guid id)
        {
            ADBackend.Instance.Users.Remove(id);
        }

        public string GenerateResetToken(string username)
        {
            return _service.GenerateResetToken(username);
        }
    }
}