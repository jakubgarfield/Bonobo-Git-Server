using System;
using System.Configuration;

namespace Bonobo.Git.Server.Configuration
{
    public static class AuthenticationSettings
    {
        public static string MembershipService { get; private set; }
        public static string AuthenticationProvider { get; private set; }
        public static bool ImportWindowsAuthUsersAsAdmin { get; private set; }
        public static string EmailDomain { get; }
        public static bool DemoModeActive { get; private set; }

        static AuthenticationSettings()
        {
            MembershipService = ConfigurationManager.AppSettings["MembershipService"];            
            AuthenticationProvider = ConfigurationManager.AppSettings["AuthenticationProvider"];
            ImportWindowsAuthUsersAsAdmin = Convert.ToBoolean(ConfigurationManager.AppSettings["ImportWindowsAuthUsersAsAdmin"]);
            EmailDomain = ConfigurationManager.AppSettings["EmailDomain"];
            DemoModeActive = Convert.ToBoolean(ConfigurationManager.AppSettings["demoModeActive"]);
        }
    }
}