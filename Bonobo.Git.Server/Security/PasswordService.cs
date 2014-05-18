using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Bonobo.Git.Server.Data;

namespace Bonobo.Git.Server.Security
{
    public class PasswordService : IPasswordService
    {
        private readonly Action<string, string> _updateUserPasswordHook;
        private readonly Func<HashAlgorithm> _getCurrentHashProvider; 
        private readonly Func<HashAlgorithm> _getDeprecatedHashProvider;

        /// <summary>
        /// Create and initialize PasswordService instance.
        /// </summary>
        /// <param name="updateUserPasswordHook">
        /// delegates the db update function with the parameters username and password.
        /// </param>
        public PasswordService(Action<string, string> updateUserPasswordHook)
        {
            if (updateUserPasswordHook == null) throw new ArgumentNullException("updateUserPasswordHook");
            _updateUserPasswordHook = updateUserPasswordHook;

            _getCurrentHashProvider = () => new SHA512CryptoServiceProvider();
            _getDeprecatedHashProvider = () => new MD5CryptoServiceProvider();
        }

        public string GetSaltedHash(string password, string salt)
        {
            var hashedSalt = GetHash(salt);
            return GetHash(GetHash(hashedSalt + password + hashedSalt));
        }

        public bool ComparePassword(string password, string salt, string hash)
        {
            if (IsOfCurrentHashFormat(hash))
            {
                return GetSaltedHash(password, salt) == hash;
            }

            // to not break backwards compatibility: use old and update
            if (GetDeprecatedHash(password) == hash)
            {
                // has to be modified if we use something other than username as salt
                var username = salt;
                UpdateToCurrentHashMethod(username, password);
                return true;
            }
            return false;
        }
        
        private bool IsOfCurrentHashFormat(string hash)
        {
            //sha512-hex, md5-hex would be 32
            return Regex.IsMatch(hash, "^[0-9A-F]{128}$");
        }
        
        private string GetDeprecatedHash(string password)
        {
            return GetHash(password, _getDeprecatedHashProvider);
        }

        private string GetHash(string content)
        {
            return GetHash(content, _getCurrentHashProvider);
        }

        private string GetHash(string content, Func<HashAlgorithm> getHashProvider)
        {
            using (var hashProvider = getHashProvider())
            {
                var data = System.Text.Encoding.UTF8.GetBytes(content);
                data = hashProvider.ComputeHash(data);
                return BitConverter.ToString(data).Replace("-", "");
            }
        }

        private void UpdateToCurrentHashMethod(string username, string password)
        {
            _updateUserPasswordHook(username, password);
        }
    }
}