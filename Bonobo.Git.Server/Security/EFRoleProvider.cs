using System;
using System.Linq;
using Bonobo.Git.Server.Data;

namespace Bonobo.Git.Server.Security
{
    public class EFRoleProvider : IRoleProvider
    {
        private readonly Func<BonoboGitServerContext> _createDatabaseContext;

        public EFRoleProvider()
        {
            _createDatabaseContext = () => new BonoboGitServerContext();
        }
        private EFRoleProvider(Func<BonoboGitServerContext> contextCreator)
        {
            _createDatabaseContext = contextCreator;
        }
        public static EFRoleProvider FromCreator(Func<BonoboGitServerContext> contextCreator)
        {
            return new EFRoleProvider(contextCreator);
        }

        public void AddUserToRoles(Guid userId, string[] roleNames)
        {
            AddUsersToRoles(new Guid[] { userId }, roleNames);
        }

        public void AddUsersToRoles(Guid[] userIds, string[] roleNames)
        {
            using (var database = _createDatabaseContext())
            {
                var roles = database.Roles.Where(i => roleNames.Contains(i.Name)).ToList();
                var users = database.Users.Where(i => userIds.Contains(i.Id)).ToList();

                foreach (var role in roles)
                {
                    foreach (var user in users)
                    {
                        role.Users.Add(user);
                    }
                }

                database.SaveChanges();
            }
        }


        public void CreateRole(string roleName)
        {
            using (var database = _createDatabaseContext())
            {
                database.Roles.Add(new Role
                {
                    Name = roleName,
                });
                database.SaveChanges();
            }
        }

        public bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            using (var database = _createDatabaseContext())
            {
                var role = database.Roles.FirstOrDefault(i => i.Name == roleName);
                if (role != null)
                {
                    if (throwOnPopulatedRole)
                    {
                        if (role.Users.Count > 0)
                        {
                            throw new InvalidOperationException("Can't delete role with members.");
                        }
                    }

                    database.Roles.Remove(role);
                    database.SaveChanges();
                    return true;
                }

                return false;
            }
        }

        public string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            using (var database = _createDatabaseContext())
            {
                var users = database.Users
                    .Where(us => us.Username.Contains(usernameToMatch) && us.Roles.Any(role => role.Name == roleName))
                    .Select(us => us.Username)
                    .ToArray();
                return users;
            }
        }

        public string[] GetAllRoles()
        {
            using (var database = _createDatabaseContext())
            {
                return database.Roles.Select(i => i.Name).ToArray();
            }
        }

        public string[] GetRolesForUser(Guid userId)
        {
            using (var database = _createDatabaseContext())
            {
                var roles = database.Roles
                    .Where(role => role.Users.Any(us => us.Id == userId))
                    .Select(role => role.Name)
                    .ToArray();
                return roles;
            }
        }

        public string[] GetUsersInRole(string roleName)
        {
            using (var database = _createDatabaseContext())
            {
                var users = database.Users
                    .Where(us => us.Roles.Any(role => role.Name == roleName))
                    .Select(us => us.Username)
                    .ToArray();
                return users;
            }
        }

        public bool IsUserInRole(Guid userId, string roleName)
        {
            using (var database = _createDatabaseContext())
            {
                return database.Roles.Any(role => role.Name == roleName && role.Users.Any(us => us.Id == userId));
            }
        }

        public void RemoveUserFromRoles(Guid userId, string[] roleNames)
        {
            RemoveUsersFromRoles(new[] { userId }, roleNames);
        }

        public void RemoveUsersFromRoles(Guid[] userIds, string[] roleNames)
        {
            using (var database = _createDatabaseContext())
            {
                var roles = database.Roles.Where(i => roleNames.Contains(i.Name)).ToList();
                var users = database.Users.Where(i => userIds.Contains(i.Id)).ToList();
                foreach (var role in roles)
                {
                    foreach (var user in users)
                    {
                        role.Users.Remove(user);
                    }
                }
                database.SaveChanges();
            }
        }

        public bool RoleExists(string roleName)
        {
            using (var database = _createDatabaseContext())
            {
                return database.Roles.Any(i => i.Name == roleName);
            }
        }
    }
}