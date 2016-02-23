using System;

namespace Bonobo.Git.Server.Security
{
    public interface IRoleProvider
    {
        void AddUserToRoles(Guid userId, string[] roleNames);
        void AddUsersToRoles(Guid[] userIds, string[] roleNames);
        void CreateRole(string roleName);
        bool DeleteRole(string roleName, bool throwOnPopulatedRole);
        string[] FindUsersInRole(string roleName, string usernameToMatch);
        string[] GetAllRoles();
        string[] GetRolesForUser(Guid userId);
        bool IsUserInRole(Guid userId, string roleName);
        string[] GetUsersInRole(string roleName);
        void RemoveUserFromRoles(Guid userId, string[] roleNames);
        void RemoveUsersFromRoles(Guid[] userIds, string[] roleNames);
        bool RoleExists(string roleName);
    }
}