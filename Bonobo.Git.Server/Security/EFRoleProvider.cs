using System;
using System.Linq;
using System.Web.Security;
using Bonobo.Git.Server.Data;

namespace Bonobo.Git.Server.Security
{
    public class EFRoleProvider : IRoleProvider
    {
        public void AddUserToRoles(string username, string[] roleNames)
        {
            AddUsersToRoles(new string[] { username }, roleNames);
        }

        public void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            using (var database = new BonoboGitServerContext())
            {
                usernames = usernames.Select(i => i.ToLowerInvariant()).ToArray();

                var roles = database.Roles.Where(i => roleNames.Contains(i.Name)).ToList();
                var users = database.Users.Where(i => usernames.Contains(i.Username)).ToList();

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
            using (var database = new BonoboGitServerContext())
            {
                database.Roles.Add(new Role
                {
                    Name = roleName,
                });
            }
        }

        public bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            using (var database = new BonoboGitServerContext())
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
            using (var database = new BonoboGitServerContext())
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
            using (var database = new BonoboGitServerContext())
            {
                return database.Roles.Select(i => i.Name).ToArray();
            }
        }

        public string[] GetRolesForUser(string username)
        {
            using (var database = new BonoboGitServerContext())
            {
                username = username.ToLowerInvariant();
                var roles = database.Roles
                    .Where(role => role.Users.Any(us => us.Username == username))
                    .Select(role => role.Name)
                    .ToArray();
                return roles;
            }
        }

        public string[] GetUsersInRole(string roleName)
        {
            using (var database = new BonoboGitServerContext())
            {
                var users = database.Users
                    .Where(us => us.Roles.Any(role => role.Name == roleName))
                    .Select(us => us.Username)
                    .ToArray();
                return users;
            }
        }

        public bool IsUserInRole(string username, string roleName)
        {
            using (var database = new BonoboGitServerContext())
            {
                username = username.ToLowerInvariant();
                bool isInRole = database.Roles.Any(role => role.Name == roleName && role.Users.Any(us => us.Username == username));
                return isInRole;
            }
        }

        public void RemoveUserFromRoles(string username, string[] roleNames)
        {
            RemoveUsersFromRoles(new string[] { username }, roleNames);
        }

        public void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            using (var database = new BonoboGitServerContext())
            {
                usernames = usernames.Select(i => i.ToLowerInvariant()).ToArray();

                var roles = database.Roles.Where(i => roleNames.Contains(i.Name)).ToList();
                var users = database.Users.Where(i => usernames.Contains(i.Username)).ToList();
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
            using (var database = new BonoboGitServerContext())
            {
                return database.Roles.Any(i => i.Name == roleName);
            }
        }
    }
}