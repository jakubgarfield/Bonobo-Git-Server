using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.Data;
using System.Data;
using Bonobo.Git.Server.Models;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace Bonobo.Git.Server.Security
{
    public class EFMembershipService : IMembershipService
    {
        private readonly Func<BonoboGitServerContext> _createDatabaseContext;
        private readonly IPasswordService _passwordService;

        public EFMembershipService()
        {
            // set up dependencies
            _createDatabaseContext = ()=>new BonoboGitServerContext();
            Action<string, string> updateUserPasswordHook =
                (username, password)=>UpdateUser(username, null, null, null, password);
            _passwordService = new PasswordService(updateUserPasswordHook);
        }

        public bool ValidateUser(string username, string password)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");

            username = username.ToLowerInvariant();
            using (var database = _createDatabaseContext())
            {
                var user = database.Users.FirstOrDefault(i => i.Username == username);
                return user != null && _passwordService.ComparePassword(password, username, user.Password);
            }
        }

        public bool CreateUser(string username, string password, string name, string surname, string email)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", "name");
            if (String.IsNullOrEmpty(surname)) throw new ArgumentException("Value cannot be null or empty.", "surname");
            if (String.IsNullOrEmpty(email)) throw new ArgumentException("Value cannot be null or empty.", "email");

            username = username.ToLowerInvariant();
            using (var database = _createDatabaseContext())
            {
                var user = new User
                {
                    Username = username,
                    Password = _passwordService.GetSaltedHash(password, username),
                    Name = name,
                    Surname = surname,
                    Email = email,
                };
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
            using (var db = _createDatabaseContext())
            {
                return db.Users.Include("Roles").ToList().Select(item => new UserModel
                {
                    Username = item.Username,
                    Name = item.Name,
                    Surname = item.Surname,
                    Email = item.Email,
                    Roles = item.Roles.Select(i => i.Name).ToArray(),
                }).ToList();
            }
        }

        public UserModel GetUser(string username)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", "username");

            username = username.ToLowerInvariant();
            using (var db = _createDatabaseContext())
            {
                var user = db.Users.FirstOrDefault(i => i.Username == username);
                return user == null ? null : new UserModel
                {
                    Username = user.Username,
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    Roles = user.Roles.Select(i => i.Name).ToArray(),
                 };
            }
        }

        public void UpdateUser(string username, string name, string surname, string email, string password)
        {
            using (var database = _createDatabaseContext())
            {
                username = username.ToLowerInvariant();
                var user = database.Users.FirstOrDefault(i => i.Username == username);
                if (user != null)
                {
                    user.Name = name ?? user.Name;
                    user.Surname = surname ?? user.Surname;
                    user.Email = email ?? user.Email;
                    user.Password = password != null ? _passwordService.GetSaltedHash(password, username) : user.Password;
                    database.SaveChanges();
                }
            }
        }

        public void DeleteUser(string username)
        {
            using (var database = _createDatabaseContext())
            {
                username = username.ToLowerInvariant();
                var user = database.Users.FirstOrDefault(i => i.Username == username);
                if (user != null)
                {
                    user.AdministratedRepositories.Clear();
                    user.Roles.Clear();
                    user.Repositories.Clear();
                    user.Teams.Clear();
                    database.Users.Remove(user);
                    database.SaveChanges();
                }
            }
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