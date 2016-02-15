﻿using System.Linq;
using Bonobo.Git.Server.Data;


namespace Bonobo.Git.Server.Security
{
    public class EFRepositoryPermissionService : IRepositoryPermissionService
    {
        public bool HasPermission(string username, string project)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(project))
                return false;

            using (var database = new BonoboGitServerContext())
            {
                username = username.ToLowerInvariant();
                var user = database.Users.FirstOrDefault(i => i.Username == username);
                if (user == null)
                {
                    // If we don't know the user, then we can't say they have permission
                    return false;
                }

                if (user.Roles.FirstOrDefault(i => i.Name == Definitions.Roles.Administrator) != null)
                {
                    // This user is an admin, so they always have permission, regardless of the repository
                    return true;
                }

                var repository = database.Repositories.FirstOrDefault(i => i.Name == project);
                if (repository != null)
                {
                    if (user.Repositories.FirstOrDefault(i => i.Name == project) != null
                    || user.AdministratedRepositories.FirstOrDefault(i => i.Name == project) != null
                    || user.Teams.Select(i => i.Name).FirstOrDefault(t => repository.Teams.Select(i => i.Name).Contains(t)) != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool AllowsAnonymous(string project)
        {
            using (var database = new BonoboGitServerContext())
            {
                var isAllowsAnonymous = database.Repositories.Any(repo => repo.Name == project && repo.Anonymous);
                return isAllowsAnonymous;
            }
        }

        public bool IsRepositoryAdministrator(string username, string project)
        {
            using (var database = new BonoboGitServerContext())
            {
                username = username.ToLowerInvariant();

                var isRepoAdmin =
                    database.Users.Where(us => us.Username == username)
                        .Any(
                            us =>
                                (us.Roles.Any(role => role.Name == Definitions.Roles.Administrator) ||
                                 us.AdministratedRepositories.Any(ar => ar.Name == project)));
                return isRepoAdmin;
            }
        }
    }
}