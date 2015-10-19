using Bonobo.Git.Server.Data;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Bonobo.Git.Server.Security
{
    public class ADRoleProvider : IRoleProvider
    {
        public void AddUserToRoles(string username, string[] roleNames)
        {
            // Use ADUC to assign groups to users instead
        }

        public void AddUsersToRoles(string[] usernames, string[] roleNames)
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

        public string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            if (String.IsNullOrEmpty(usernameToMatch)) throw new ArgumentException("Value cannot be null or empty.", "usernameToMatch");

            return String.IsNullOrEmpty(usernameToMatch) ? ADBackend.Instance.Roles[roleName].Members : ADBackend.Instance.Roles[roleName].Members.Where(x => x.Contains(usernameToMatch)).ToArray();
        }

        public string[] GetAllRoles()
        {
            return ADBackend.Instance.Roles.Select(x => x.Name).ToArray();
        }

        public string[] GetRolesForUser(string username)
        {
            return ADBackend.Instance.Roles.Where(x => x.Members.Contains(username, StringComparer.OrdinalIgnoreCase)).Select(x => x.Name).ToArray();
        }

        public string[] GetUsersInRole(string roleName)
        {
            return ADBackend.Instance.Roles[roleName].Members;
        }

        public bool IsUserInRole(string username, string roleName)
        {
            return ADBackend.Instance.Roles[roleName].Members.Contains(username, StringComparer.OrdinalIgnoreCase);
        }

        public void RemoveUserFromRoles(string username, string[] roleNames)
        {
            // Use ADUC to remove users from groups
        }

        public void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            // Use ADUC to remove users from groups
        }

        public bool RoleExists(string roleName)
        {
            return ADBackend.Instance.Roles[roleName] != null;
        }
    }
}