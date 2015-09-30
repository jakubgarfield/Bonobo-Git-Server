using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Security
{
    public interface IRoleProvider
    {
        void AddUserToRoles(string username, string[] roleNames);
        void AddUsersToRoles(string[] usernames, string[] roleNames);
        void CreateRole(string roleName);
        bool DeleteRole(string roleName, bool throwOnPopulatedRole);
        string[] FindUsersInRole(string roleName, string usernameToMatch);
        string[] GetAllRoles();
        string[] GetRolesForUser(string username);
        string[] GetUsersInRole(string roleName);
        void RemoveUserFromRoles(string username, string[] roleNames);
        void RemoveUsersFromRoles(string[] username, string[] roleNames);
        bool RoleExists(string roleName);
    }
}