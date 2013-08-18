using Bonobo.Git.Server.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Bonobo.Git.Server
{
    public class WindowsIdentityImporter
    {
        public void Import(WindowsIdentity identity)
        {
            var service = new EFMembershipService();
            if (service.GetUser(identity.User.Value) == null)
            {
                service.CreateUser(identity.User.Value, "imported", identity.Name, "None", "None");

                if (!String.Equals(ConfigurationManager.AppSettings["ShouldImportWindowsUserAsAdministrator"], "true", StringComparison.InvariantCultureIgnoreCase))
                    return;

                var roleProvider = new EFRoleProvider();
                roleProvider.AddUsersToRoles(new[] { identity.User.Value }, new[] { Definitions.Roles.Administrator });
            }
        }
    }
}