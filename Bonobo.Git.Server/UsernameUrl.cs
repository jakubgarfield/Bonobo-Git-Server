using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Bonobo.Git.Server
{
    public class UsernameUrl
    {
        //to allow support for email addresses as user names, only encode/decode user name if it is not an email address
        private static Regex _isEmailRegEx = new Regex(
            @"^(([A-Za-z0-9]+_+)|([A-Za-z0-9]+\-+)|([A-Za-z0-9]+\.+)|([A-Za-z0-9]+\++))*[A-Za-z0-9]+@((\w+\-+)|(\w+\.))*\w{1,63}\.[a-zA-Z]{2,6}$",
            RegexOptions.Compiled);

        public static string Encode(string username)
        {
            var nameParts = username.Split('\\');
            if ( nameParts.Count() == 2  && !_isEmailRegEx.IsMatch(username) )
            {
                return nameParts[1] + "@" + nameParts[0];
            }

            return username;
        }

        public static string Decode(string username)
        {
            var nameParts = username.Split('@');
            if ( (nameParts.Count() == 2) && (!_isEmailRegEx.IsMatch(username) ||
                 (String.Equals(ConfigurationManager.AppSettings["ActiveDirectoryIntegration"], "true", StringComparison.InvariantCultureIgnoreCase))))
            {
                return nameParts[1] + "\\" + nameParts[0];
            }

            return username;
        }
    }
}