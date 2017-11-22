using System;
using System.Linq;
using Bonobo.Git.Server.Data;

namespace Bonobo.Git.Server.Security
{
    public class EFRoleProvider : IRoleProvider
    {
        private BonoboGitServerContext _ctx;
        public EFRoleProvider(BonoboGitServerContext createContext)
        {
            _ctx = createContext;
        }

        public BonoboGitServerContext CreateContext() => _ctx;

        public void AddUserToRoles(Guid userId, string[] roleNames)
        {
            AddUsersToRoles(new Guid[] { userId }, roleNames);
        }

        public void AddUsersToRoles(Guid[] userIds, string[] roleNames)
        {
            var database = CreateContext();
            {
                var roles = database.Roles.Where(i => roleNames.Contains(i.Name)).ToList();
                var users = database.Users.Where(i => userIds.Contains(i.Id)).ToList();

                foreach (var role in roles)
                {
                    foreach (var user in users)
                    {
                        var userRole = new UserRole
                        {
                            RoleId = role.Id,
                            UserId = user.Id,
                        };
                        role.Users.Add(userRole);
                    }
                }

                database.SaveChanges();
            }
        }


        public void CreateRole(string roleName)
        {
            var database = CreateContext();
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
            var database = CreateContext();
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
            var database = CreateContext();
            {
                var users = database.Users
                    .Where(us => us.Username.Contains(usernameToMatch) && us.Roles.Any(role => role.Role.Name == roleName))
                    .Select(us => us.Username)
                    .ToArray();
                return users;
            }
        }

        public string[] GetAllRoles()
        {
            var database = CreateContext();
            {
                return database.Roles.Select(i => i.Name).ToArray();
            }
        }

        public string[] GetRolesForUser(Guid userId)
        {
            var database = CreateContext();
            {
                var roles = database.Roles
                    .Where(role => role.Users.Any(us => us.UserId == userId))
                    .Select(role => role.Name)
                    .ToArray();
                return roles;
            }
        }

        public Guid[] GetUsersInRole(string roleName)
        {
            var database = CreateContext();
            {
                var users = database.Users
                    .Where(us => us.Roles.Any(role => role.Role.Name == roleName))
                    .Select(us => us.Id)
                    .ToArray();
                return users;
            }
        }

        public bool IsUserInRole(Guid userId, string roleName)
        {
            var database = CreateContext();
            {
                return database.Roles.Any(role => role.Name == roleName && role.Users.Any(us => us.UserId == userId));
            }
        }

        public void RemoveUserFromRoles(Guid userId, string[] roleNames)
        {
            RemoveUsersFromRoles(new[] { userId }, roleNames);
        }

        public void RemoveUsersFromRoles(Guid[] userIds, string[] roleNames)
        {
            var database = CreateContext();
            {
                var roles = database.Roles.Where(i => roleNames.Contains(i.Name)).ToList();
                var users = database.Users.Where(i => userIds.Contains(i.Id)).ToList();
                foreach (var role in roles)
                {
                    foreach (var user in users)
                    {
                        var userRole = new UserRole
                        {
                            RoleId = role.Id,
                            UserId = user.Id,
                        };
                        role.Users.Remove(userRole);
                    }
                }
                database.SaveChanges();
            }
        }

        public bool RoleExists(string roleName)
        {
            var database = CreateContext();
            {
                return database.Roles.Any(i => i.Name == roleName);
            }
        }
    }
}