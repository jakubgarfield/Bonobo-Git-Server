using Bonobo.Git.Server.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace Bonobo.Git.Server
{
    public class WindowsIdentityImporter
    {
        public void Import(AuthorizationContext context)
        {
            var windowsIdentity = context.HttpContext.User.Identity as WindowsIdentity;
            if (windowsIdentity == null)
                return;

            else if (windowsIdentity.IsAuthenticated)
            {
                Import(windowsIdentity);
            }
        }

        public void Import(WindowsIdentity identity)
        {
            var service = new EFMembershipService();
            if (service.GetUser(identity.Name) == null)
            {
                service.CreateUser(identity.Name, "imported", identity.Name, "None", "None");

                if (!String.Equals(ConfigurationManager.AppSettings["ShouldImportWindowsUserAsAdministrator"], "true", StringComparison.InvariantCultureIgnoreCase))
                    return;

                var roleProvider = new EFRoleProvider();
                roleProvider.AddUsersToRoles(new[] { identity.Name }, new[] { Definitions.Roles.Administrator });
            }
        }
    }
}