using Bonobo.Git.Server.Data;
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
        public static void Import(AuthorizationContext context)
        {
            var windowsIdentity = context.HttpContext.User.Identity as WindowsIdentity;
            if (windowsIdentity == null)
            {
                return;
            }

            if (windowsIdentity.IsAuthenticated)
            {
                Import(windowsIdentity);
            }
        }

        private static void Import(WindowsIdentity identity)
        {
            var service = new EFMembershipService();
            if (service.GetUser(identity.Name) != null)
            {
                RefreshTeams(identity);
                return;
            }

            service.CreateUser(identity.Name, "imported", identity.Name, "None", "None");
            RefreshTeams(identity);

            if (
                !String.Equals(ConfigurationManager.AppSettings["ShouldImportWindowsUserAsAdministrator"],
                               "true",
                               StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var roleProvider = new EFRoleProvider();
            roleProvider.AddUsersToRoles(new[] { identity.Name }, new[] { Definitions.Roles.Administrator });
        }

        private static void RefreshTeams(WindowsIdentity identity)
        {
            if (!String.Equals(ConfigurationManager.AppSettings["ActiveDirectoryIntegration"], "true", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            if (identity.Groups != null)
            {
                var groups = identity.Groups.Select(@group =>
                {
                    IdentityReference translated = null;
                    try
                    {
                        if (@group != null)
                        {
                            translated = @group.Translate(typeof(NTAccount));
                        }
                    }

                    catch (System.Security.Principal.IdentityNotMappedException) { }

                    return translated == null ? null : translated.ToString();
                })
                .Where(s => string.IsNullOrWhiteSpace(s) == false)
                .ToList();

                var teamRepository = new EFTeamRepository();
                var teams = teamRepository.GetAllTeams();

                // Get current teams
                var newTeams = teams.Where(t => t.Members.Contains(identity.Name)).Select(t => t.Name).ToList();
                bool isChanged = false;

                // Remove non matching group
                foreach (var team in newTeams)
                {
                    var group = groups.SingleOrDefault(t => t == team);
                    if (group == null)
                    {
                        newTeams.Remove(team);
                        isChanged = true;
                    }
                }

                // Insert new matching group
                foreach (var group in groups)
                {
                    var team = teams.SingleOrDefault(t => t.Name == group);
                    if (team != null && newTeams.All(t => t != identity.Name))
                    {
                        newTeams.Add(team.Name);
                        isChanged = true;
                    }
                }

                if (isChanged)
                {
                    teamRepository.UpdateUserTeams(identity.Name, newTeams);
                }
            }
        }
    }
}