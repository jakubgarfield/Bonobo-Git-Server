using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Helpers
{
    public class UsernameHelper
    {

        /// <summary>
        /// Replaces first occurrence of oldChar with newChar
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string ReplaceFirstOccurrence(string username, char oldChar, char newChar)
        {
            if (!string.IsNullOrEmpty(username))
            {
                int index = username.IndexOf(oldChar);
                if (index > 0)
                {
                    char[] array = username.ToCharArray();
                    array[index] = newChar;
                    return new string(array);
                }
            }
            return username;
        }
    }
}