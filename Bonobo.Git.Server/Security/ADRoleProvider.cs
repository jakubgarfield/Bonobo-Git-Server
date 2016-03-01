using Bonobo.Git.Server.Data;
using System;
using System.Linq;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Security
{
    public class ADRoleProvider : IRoleProvider
    {
        public void AddUserToRoles(Guid userId, string[] roleNames)
        {
            // Use ADUC to assign groups to users instead
        }

        public void AddUsersToRoles(Guid[] userIds, string[] roleNames)
        {
            // Use ADUC to assign groups to users instead
        }

        public void CreateRole(string roleName)
        {
            // Use ADUC to create groups
        }

        public bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            // Use ADUC to remove groups
            return false;
        }

        public string[] GetAllRoles()
        {
            return ADBackend.Instance.Roles.Select(x => x.Name).ToArray();
        }

        public string[] GetRolesForUser(Guid userId)
        {
            var user = ADBackend.Instance.Users.First(x => x.Id == userId);
            return ADBackend.Instance.Roles.Where(x => x.Members.Contains(user.Id)).Select(x => x.Name).ToArray();
        }

        public Guid[] GetUsersInRole(string roleName)
        {
            return GetRoleByName(roleName).Members;
        }

        public bool IsUserInRole(Guid userId, string roleName)
        {
            var user = ADBackend.Instance.Users.First(x => x.Id == userId);
            return GetRoleByName(roleName).Members.Contains(user.Id);
        }

        public void RemoveUserFromRoles(Guid userId, string[] roleNames)
        {
            // Use ADUC to remove users from groups
        }

        public void RemoveUsersFromRoles(Guid[] userIds, string[] roleNames)
        {
            // Use ADUC to remove users from groups
        }

        public bool RoleExists(string roleName)
        {
            return ADBackend.Instance.Roles.Any(role => role.Name == roleName);
        }

        private static RoleModel GetRoleByName(string roleName)
        {
            return ADBackend.Instance.Roles.First(role => role.Name == roleName);
        }
    }
}