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
        public void Import(WindowsIdentity indentity)
        {
            var service = new EFMembershipService();
            if (service.GetUser(indentity.Name) == null)
            {
                service.CreateUser(indentity.Name, "imported", "None", "None", "None");

                if (!String.Equals(ConfigurationManager.AppSettings["ShouldImportWindowsUserAsAdministrator"], "true", StringComparison.InvariantCultureIgnoreCase))
                    return;

                var roleProvider = new EFRoleProvider();
                roleProvider.AddUsersToRoles(new[] { indentity.Name }, new[] { Definitions.Roles.Administrator });
            }
        }
    }
}