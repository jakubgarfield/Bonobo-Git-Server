using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server
{
    public class UsernameUrl
    {
        public static string Encode(string username)
        {
            var nameParts = username.Split('\\');
            if (nameParts.Count() == 2)
            {
                return nameParts[1] + "@" + nameParts[0];
            }

            return username;
        }

        public static string Decode(string username)
        {
            var nameParts = username.Split('@');
            if (nameParts.Count() == 2)
            {
                return nameParts[1] + "\\" + nameParts[0];
            }

            return username;
        }
    }
}