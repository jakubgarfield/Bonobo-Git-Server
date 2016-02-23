using System.Linq;
using Bonobo.Git.Server.Data;
using System;
using Microsoft.Practices.Unity;

namespace Bonobo.Git.Server.Security
{
    public class EFRepositoryPermissionService : IRepositoryPermissionService
    {
        [Dependency]
        public IRepositoryRepository Repository { get; set; }

        public bool HasPermission(Guid userId, string repositoryName)
        {
            return HasPermission(userId, Repository.GetRepository(repositoryName).Id);
        }

        public bool HasPermission(Guid userId, Guid repositoryId)
        {
            using (var database = new BonoboGitServerContext())
            {
                var user = database.Users.FirstOrDefault(i => i.Id == userId);
                var repository = database.Repositories.FirstOrDefault(i => i.Id == repositoryId);
                if (user != null && repository != null)
                {
                    if (user.Roles.FirstOrDefault(i => i.Name == Definitions.Roles.Administrator) != null
                     || user.Repositories.FirstOrDefault(i => i.Id == repositoryId) != null
                     || user.AdministratedRepositories.FirstOrDefault(i => i.Id == repositoryId) != null
                     || user.Teams.Select(i => i.Name).FirstOrDefault(t => repository.Teams.Select(i => i.Name).Contains(t)) != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool AllowsAnonymous(string repositoryName)
        {
            using (var database = new BonoboGitServerContext())
            {
                var isAllowsAnonymous = database.Repositories.Any(repo => repo.Name == repositoryName && repo.Anonymous);
                return isAllowsAnonymous;
            }
        }

        public bool AllowsAnonymous(Guid repositoryId)
        {
            using (var database = new BonoboGitServerContext())
            {
                var isAllowsAnonymous = database.Repositories.Any(repo => repo.Id == repositoryId && repo.Anonymous);
                return isAllowsAnonymous;
            }
        }

        public bool IsRepositoryAdministrator(Guid userId, Guid repositoryId)
        {
            using (var database = new BonoboGitServerContext())
            {
                var isRepoAdmin =
                    database.Users.Where(us => us.Id == userId)
                        .Any(
                            us =>
                                (us.Roles.Any(role => role.Name == Definitions.Roles.Administrator) ||
                                 us.AdministratedRepositories.Any(ar => ar.Id == repositoryId)));
                return isRepoAdmin;
            }
        }

        public bool HasPermission(string username, string password, string repositoryName)
        {
            throw new NotImplementedException();
        }
    }
}