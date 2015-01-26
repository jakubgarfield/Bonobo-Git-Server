using System.Web.Mvc;
using System.Web.Security;
using Bonobo.Git.Server.Security;

namespace Bonobo.Git.Server
{
    public class FormsIdentityImporter
    {
        public static void Import(AuthorizationContext context)
        {
            var windowsIdentity = context.HttpContext.User.Identity as FormsIdentity;
            if (windowsIdentity == null)
            {
                return;
            }

            if (windowsIdentity.IsAuthenticated)
            {
                Import(windowsIdentity);
            }
        }

        private static void Import(FormsIdentity identity)
        {
            var service = new EFMembershipService();
            var user = service.GetUser(identity.Name);
            if (user != null)
            {
                RefreshTeams(identity);
                return;
            }

            service.CreateUser(identity.Name, "imported", identity.Name, "None", "None");
            RefreshTeams(identity);
            
            //var roleProvider = new EFRoleProvider();
            //roleProvider.AddUsersToRoles(new[] { identity.Name }, new[] { Definitions.Roles.Administrator });
        }

        private static void RefreshTeams(FormsIdentity identity)
        {
            //if (!String.Equals(ConfigurationManager.AppSettings["ActiveDirectoryIntegration"], "true", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    return;
            //}

            //if (identity.Groups != null)
            //{
            //    var groups = identity.Groups.Select(@group => @group.Translate(typeof (NTAccount)).ToString()).ToList();

            //    var teamRepository = new EFTeamRepository();
            //    var teams = teamRepository.GetAllTeams();

            //    // Get current teams
            //    var newTeams = teams.Where(t => t.Members.Contains(identity.Name)).Select(t => t.Name).ToList();
            //    bool isChanged = false;

            //    // Remove non matching group
            //    foreach (var team in newTeams)
            //    {
            //        var group = groups.SingleOrDefault(t => t == team);
            //        if (group == null)
            //        {
            //            newTeams.Remove(team);
            //            isChanged = true;
            //        }
            //    }

            //    // Insert new matching group
            //    foreach (var group in groups)
            //    {
            //        var team = teams.SingleOrDefault(t => t.Name == group);
            //        if (team != null && newTeams.All(t => t != identity.Name))
            //        {
            //            newTeams.Add(team.Name);
            //            isChanged = true;
            //        }
            //    }

            //    if (isChanged)
            //    {
            //        teamRepository.UpdateUserTeams(identity.Name, newTeams);
            //    }
            //}
        }
    }
}