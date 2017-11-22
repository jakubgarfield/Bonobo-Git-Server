using System;
using System.Linq;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Security
{
    public class ADRoleProvider : IRoleProvider
    {
        private readonly ADBackend _adBackend;

        public ADRoleProvider(ADBackend adBackend)
        {
            this._adBackend = adBackend;
        }
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
            return _adBackend.Roles.Select(x => x.Name).ToArray();
        }

        public string[] GetRolesForUser(Guid userId)
        {
            var user = _adBackend.Users.FirstOrDefault(x => x.Id == userId);
            if (user == null)
            {
                // This is how the EF provider works
                return new string[0];
            }
            return _adBackend.Roles.Where(x => x.Members.Contains(user.Id)).Select(x => x.Name).ToArray();
        }

        public Guid[] GetUsersInRole(string roleName)
        {
            return GetRoleByName(roleName).Members;
        }

        public bool IsUserInRole(Guid userId, string roleName)
        {
            var user = _adBackend.Users.First(x => x.Id == userId);
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
            return _adBackend.Roles.Any(role => role.Name == roleName);
        }

        private RoleModel GetRoleByName(string roleName)
        {
            return _adBackend.Roles.First(role => role.Name == roleName);
        }
    }
}