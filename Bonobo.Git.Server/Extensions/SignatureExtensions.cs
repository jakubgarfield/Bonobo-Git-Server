using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LibGit2Sharp;
using System.Security.Cryptography;
using System.Text;

namespace Bonobo.Git.Server.Extensions
{
    public static class SignatureExtensions
    {
        static Dictionary<string, string> avatars = new Dictionary<string, string>();

        public static string GetAvatar(this Signature signature, int size = 75)
        {
            string key = signature.Email + "_" + size;
            if (!avatars.ContainsKey(key))
            {
                string avatar = "//www.gravatar.com/avatar/";
                MD5 md5Hasher = MD5.Create();
                byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(signature.Email));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    builder.Append(data[i].ToString("x2"));
                }
                avatar += builder.ToString() + "?s=" + size;
                avatars[key] = avatar;
            }
            return avatars[key];
        }
    }
}