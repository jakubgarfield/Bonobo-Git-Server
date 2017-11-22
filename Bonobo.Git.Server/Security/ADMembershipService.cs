using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Helpers;
using Bonobo.Git.Server.Models;
using Serilog;

namespace Bonobo.Git.Server.Security
{
    public class ADMembershipService : IMembershipService
    {
        private readonly ADHelper _adHelper;
        private readonly ADBackend _adBackend;

        public ADMembershipService(ADHelper adHelper, ADBackend adBackend)
        {
            _adHelper = adHelper;
            _adBackend = adBackend;
        }

        public bool IsReadOnly()
        {
            return true;
        }

        public ValidationResult ValidateUser(string username, string password)
        {
            ValidationResult result = ValidationResult.Failure;

            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty", "username");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty", "password");

            try
            {
                if (_adHelper.ValidateUser(username, password))
                {
                    using (var user = _adHelper.GetUserPrincipal(username))
                    using (var pc = _adHelper.GetMembersGroup(out GroupPrincipal group))
                    {
                        if (group == null)
                            result = ValidationResult.Failure;

                        if (user != null)
                        {
                            if (!group.GetMembers(true).Contains(user))
                            {
                                result = ValidationResult.NotAuthorized;
                            }
                            else
                            {
                                result = ValidationResult.Success;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AD.ValidateUser Exception: ");
                result = ValidationResult.Failure;
            }

            return result;
        }

        public bool CreateUser(string username, string password, string givenName, string surname, string email, Guid id)
        {
            return false;
        }

        public bool CreateUser(string username, string password, string givenName, string surname, string email)
        {
            return false;
        }

        public IList<UserModel> GetAllUsers()
        {
            var users = _adBackend.Users.ToList();
            return users;
        }

        public UserModel GetUserModel(string username)
        {
            using (var upc = _adHelper.GetUserPrincipal(username))
            {
                return _adBackend.Users.FirstOrDefault(n => n.Id == upc.Guid.Value);
            }
            throw new ArgumentException("User was not found with username: " + username);
        }

        public UserModel GetUserModel(Guid id)
        {
            return _adBackend.Users[id];
        }

        private static bool UsernameContainsDomain(string username)
        {
            return String.IsNullOrEmpty(username) && !string.IsNullOrEmpty(username.GetDomain());
        }

        public void UpdateUser(Guid id, string username, string givenName, string surname, string email, string password)
        {
            throw new NotSupportedException();
        }

        public void DeleteUser(Guid id)
        {
            throw new NotSupportedException();
        }

        public string GenerateResetToken(string username)
        {
            throw new NotSupportedException();
        }
    }
}
