using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Data.Entity.Core;

using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using System.Web.Security;
using System.Security.Principal;
using Bonobo.Git.Server.Configuration;

namespace Bonobo.Git.Server.Security
{
    public class ADMembershipService : IMembershipService
    {
        private static EFMembershipService userRepository = new EFMembershipService();

        public bool IsReadOnly()
        {
            return true;
        }

        public ValidationResult ValidateUser(string username, string password)
        {
            ValidationResult result = ValidationResult.Failure;

            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty", "password");

            try
            {
                string domain = GetDomainFromUsername(username);
                if (String.IsNullOrEmpty(domain))
                {
                    domain = Configuration.ActiveDirectorySettings.DefaultDomain;
                }

                using (PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, domain))
                {
                    if (principalContext.ValidateCredentials(username, password))
                    {
                        using (UserPrincipal user = UserPrincipal.FindByIdentity(principalContext, username))
                        {
							using (GroupPrincipal group = GroupPrincipal.FindByIdentity(principalContext, Configuration.ActiveDirectorySettings.MemberGroupName))
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
										ADBackend.Instance.Users.AddOrUpdate(new UserModel
										{
											Name = user.UserPrincipalName,
											GivenName = user.GivenName ?? String.Empty,
											Surname = user.Surname ?? String.Empty,
											Email = user.EmailAddress ?? String.Empty,
										});
										result = ValidationResult.Success;
									}
								}
							}
                        }
                    }
                }
            }
            catch
            {
                result = ValidationResult.Failure;
            }

            return result;
        }

        private Dictionary<int, string> _id_to_name = null;

        public bool CreateUser(string username, string password, string name, string surname, string email)
        {
            return false;
        }

        public IList<UserModel> GetAllUsers()
        {
            var users = ADBackend.Instance.Users.ToList();
            foreach (var user in users)
            {
                _id_to_name[user.Id] = user.Name;
            }
            return users;
        }

        public UserModel GetUser(string username)
        {
            string domain = GetDomainFromUsername(username);
            if (!IsUserPrincipalName(username))
            {
                using (PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, ActiveDirectorySettings.DefaultDomain))
                using (UserPrincipal user = UserPrincipal.FindByIdentity(principalContext, username))
                {
                    username = user.UserPrincipalName;
                }
            }

            return ADBackend.Instance.Users.FirstOrDefault(n=>n.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public UserModel GetUser(int id)
        {
            if (_id_to_name == null){
                _id_to_name = new Dictionary<int, string>();
                GetAllUsers();
            }
            return GetUser(_id_to_name[id]);
        }

        private static bool IsUserPrincipalName(string username)
        {
            bool result = false;

            if (!String.IsNullOrEmpty(username))
            {
                int atIndex = username.IndexOf('@');
                result = atIndex > 0 && atIndex < username.Length - 1;
            }

            return result;
        }

        public void UpdateUser(int id, string username, string name, string surname, string email, string password)
        {
            throw new NotImplementedException();
        }

        public void DeleteUser(string username)
        {
            throw new NotImplementedException();
        }

        public string GenerateResetToken(string username)
        {
            throw new NotImplementedException();
        }

        private string GetDomainFromUsername(string username)
        {
            string result = null;

            int length = username.Length;
            int separatorPosition = username.IndexOf('\\');
            if (separatorPosition > 0 && separatorPosition < length)
            {
                result = username.Substring(0, separatorPosition);
            }
            else
            {
                separatorPosition = username.IndexOf('@');
                if (separatorPosition > 0 && separatorPosition < length)
                {
                    result = username.Substring(separatorPosition + 1);
                }
            }

            return result;
        }
    }
}