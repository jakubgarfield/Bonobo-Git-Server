using System;
using System.Linq;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.ManyToMany;
using Bonobo.Git.Server.Extensions;

namespace Bonobo.Git.Server.Security
{
    public class EFRoleProvider : IRoleProvider
    {
        public Func<BonoboGitServerContext> CreateContext { get; set; }

        public EFRoleProvider(Func<BonoboGitServerContext> createContext)
        {
            CreateContext = createContext;
        }

        public void AddUserToRoles(Guid userId, string[] roleNames)
        {
            AddUsersToRoles(new Guid[] { userId }, roleNames);
        }

        public void AddUsersToRoles(Guid[] userIds, string[] roleNames)
        {
            using (var database = CreateContext())
            {
                var roles = database.Roles.Where(i => roleNames.Contains(i.Name)).ToList();
                var users = database.Users.Where(i => userIds.Contains(i.Id)).ToList();

                foreach (var role in roles)
                {
                    foreach (var user in users)
                    {
                        role.Users.Add(new UserRole_InRole
                        {
                            Role = role,
                            User = user
                        });
                    }
                }

                database.SaveChanges();
            }
        }


        public void CreateRole(string roleName)
        {
            using (var database = CreateContext())
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
            using (var database = CreateContext())
            {
                var role = database.Roles.FirstOrDefault(i => i.Name == roleName);
                if (role != null)
                {
                    if (throwOnPopulatedRole)
                    {
                        if (role.Users.Any())
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
            using (var database = CreateContext())
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
            using (var database = CreateContext())
            {
                return database.Roles.Select(i => i.Name).ToArray();
            }
        }

        public string[] GetRolesForUser(Guid userId)
        {
            using (var database = CreateContext())
            {
                var sql = database.Roles
                    .Where(role => role.Users.Any(us => us.UserId == userId))
                    .Select(role => role.Name).ToSql();

                var roles = database.Roles
                    .Where(role => role.Users.Any(us => us.UserId == userId))
                    .Select(role => role.Name)
                    .ToArray();
                return roles;
            }
        }

        public Guid[] GetUsersInRole(string roleName)
        {
            using (var database = CreateContext())
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
            using (var database = CreateContext())
            {
                return database.Roles.Any(role => role.Name == roleName && role.Users.Any(us => us.User.Id == userId));
            }
        }

        public void RemoveUserFromRoles(Guid userId, string[] roleNames)
        {
            RemoveUsersFromRoles(new[] { userId }, roleNames);
        }

        public void RemoveUsersFromRoles(Guid[] userIds, string[] roleNames)
        {
            using (var database = CreateContext())
            {
                var roles = database.Roles.Where(i => roleNames.Contains(i.Name)).ToList();
                var users = database.Users.Where(i => userIds.Contains(i.Id)).ToList();
                foreach (var role in roles)
                {
                    foreach (var user in users)
                    {
                        database.Remove(new UserRole_InRole
                        {
                            RoleId = role.Id,
                            UserId = user.Id
                        });
                    }
                }
                database.SaveChanges();
            }
        }

        public bool RoleExists(string roleName)
        {
            using (var database = CreateContext())
            {
                return database.Roles.Any(i => i.Name == roleName);
            }
        }
    }
}