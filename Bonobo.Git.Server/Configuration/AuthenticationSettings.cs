using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Configuration
{
    public class AuthenticationSettings
    {
        public static string MembershipService { get; private set; }

        static AuthenticationSettings()
        {
            MembershipService = ConfigurationManager.AppSettings["MembershipService"];
        }
    }
}