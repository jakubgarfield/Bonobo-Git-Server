using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using System.Security.Cryptography;
using System.Data.Entity.Core;
using Microsoft.Practices.Unity;

namespace Bonobo.Git.Server.Security
{
    public class EFMembershipService : IMembershipService
    {
        [Dependency]
        public Func<BonoboGitServerContext> CreateContext { get; set; }

        private readonly IPasswordService _passwordService;

        public EFMembershipService()
        {
            // set up dependencies
            Action<string, string> updateUserPasswordHook =
                (username, password) =>
                {
                    using (var db = CreateContext())
                    {
                        var user = db.Users.FirstOrDefault(i => i.Username == username);
                        if (user != null)
                        {
                            UpdateUser(user.Id, null, null, null, null, password);
                        }
                    }
                };
            _passwordService = new PasswordService(updateUserPasswordHook);
        }

        public bool IsReadOnly()
        {
            return false;
        }

        public ValidationResult ValidateUser(string username, string password)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", "username");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");

            username = username.ToLowerInvariant();
            using (var database = CreateContext())
            {
                var user = database.Users.FirstOrDefault(i => i.Username == username);
                return user != null && _passwordService.ComparePassword(password, username, user.PasswordSalt, user.Password) ? ValidationResult.Success : ValidationResult.Failure;
            }
        }

        public bool CreateUser(string username, string password, string givenName, string surname, string email)
        {
            return CreateUser(username, password, givenName, surname, email, Guid.NewGuid());
        }

        public bool CreateUser(string username, string password, string givenName, string surname, string email, Guid id)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", "username");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
            if (String.IsNullOrEmpty(givenName)) throw new ArgumentException("Value cannot be null or empty.", "givenName");
            if (String.IsNullOrEmpty(surname)) throw new ArgumentException("Value cannot be null or empty.", "surname");
            if (String.IsNullOrEmpty(email)) throw new ArgumentException("Value cannot be null or empty.", "email");
            if (id == Guid.Empty) throw new ArgumentException("Id must be a proper Guid", "id");

            username = username.ToLowerInvariant();
            using (var database = CreateContext())
            {
                var user = new User
                {
                    Id = id,
                    Username = username,
                    GivenName = givenName,
                    Surname = surname,
                    Email = email,
                };
                SetPassword(user, password);
                database.Users.Add(user);
                try
                {
                    database.SaveChanges();
                }
                catch (UpdateException)
                {
                    return false;
                }
            }

            return true;
        }

        public IList<UserModel> GetAllUsers()
        {
            using (var db = CreateContext())
            {
                return db.Users.Include("Roles").ToList().Select(item => new UserModel
                {
                    Id = item.Id,
                    Username = item.Username,
                    GivenName = item.GivenName,
                    Surname = item.Surname,
                    Email = item.Email,
                }).ToList();
            }
        }

        public int UserCount()
        {
            using (var db = CreateContext())
            {
                return db.Users.Count();
            }
        }

        private UserModel GetUserModel(User user)
        {
            return user == null ? null : new UserModel
            {
                Id = user.Id,
                Username = user.Username,
                GivenName = user.GivenName,
                Surname = user.Surname,
                Email = user.Email,
             };
        }

        public UserModel GetUserModel(Guid id)
        {
            using (var db = CreateContext())
            {
                var user = db.Users.FirstOrDefault(i => i.Id == id);
                return GetUserModel(user);
            }
        }

        public UserModel GetUserModel(string username)
        {
            using (var db = CreateContext())
            {
                username = username.ToLowerInvariant();
                var user = db.Users.FirstOrDefault(i => i.Username == username);
                return GetUserModel(user);
            }
        }

        public void UpdateUser(Guid id, string username, string givenName, string surname, string email, string password)
        {
            using (var db = CreateContext())
            {
                var user = db.Users.FirstOrDefault(i => i.Id == id);
                if (user != null)
                {
                    var lowerUsername = username == null ? null : username.ToLowerInvariant();
                    user.Username = lowerUsername ?? user.Username;
                    user.GivenName = givenName ?? user.GivenName;
                    user.Surname = surname ?? user.Surname;
                    user.Email = email ?? user.Email;
                    if (password != null)
                    {
                        SetPassword(user, password);
                    }
                    db.SaveChanges();
                }
            }
        }

        public void DeleteUser(Guid id)
        {
            using (var db = CreateContext())
            {
                foreach (var user in db.Users)
                {
                    if (user.Id == id)
                    {
                        user.AdministratedRepositories.Clear();
                        user.Roles.Clear();
                        user.Repositories.Clear();
                        user.Teams.Clear();
                        db.Users.Remove(user);
                        db.SaveChanges();
                    }
                }
            }
        }

        private void SetPassword(User user, string password)
        {
            if (user == null) throw new ArgumentNullException("user", "User cannot be null");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Password cannot be null or empty.", "password");

            user.PasswordSalt = Guid.NewGuid().ToString();
            user.Password = _passwordService.GetSaltedHash(password, user.PasswordSalt);
        }

        private const int PBKDF2IterCount = 1000; // default for Rfc2898DeriveBytes
        private const int PBKDF2SubkeyLength = 256 / 8; // 256 bits
        private const int SaltSize = 128 / 8; // 128 bits

        public string GenerateResetToken(string username)
        {
            byte[] salt;
            byte[] subkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(username, SaltSize, PBKDF2IterCount))
            {
                salt = deriveBytes.Salt;
                subkey = deriveBytes.GetBytes(PBKDF2SubkeyLength);
            }

            byte[] outputBytes = new byte[1 + SaltSize + PBKDF2SubkeyLength];
            Buffer.BlockCopy(salt, 0, outputBytes, 1, SaltSize);
            Buffer.BlockCopy(subkey, 0, outputBytes, 1 + SaltSize, PBKDF2SubkeyLength);
            return Convert.ToBase64String(outputBytes);
        }        
    }
}