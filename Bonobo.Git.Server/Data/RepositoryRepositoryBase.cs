using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Data
{
    public abstract class RepositoryRepositoryBase : IRepositoryRepository
    {
        public IList<RepositoryModel> GetPermittedRepositories(Guid userId, Guid[] userTeamsId)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Do not pass invalid userId", "userId");
            return GetAllRepositories().Where(repo =>
                repo.Users.Any(user => user.Id == userId) ||
                repo.Administrators.Any(admin => admin.Id == userId) ||
                repo.Teams.Any(team => userTeamsId.Contains(team.Id)) ||
                repo.AnonymousAccess).ToList();
        }

        public virtual IList<RepositoryModel> GetTeamRepositories(Guid[] teamsId)
        {
            return GetAllRepositories().Where(repo => repo.Teams.Any(team => teamsId.Contains(team.Id))).ToList();
        }

        public virtual IList<RepositoryModel> GetAdministratedRepositories(Guid userId)
        {
            return GetAllRepositories().Where(x => x.Administrators.Any(y => y.Id == userId)).ToList();
        }

        public abstract RepositoryModel GetRepository(Guid id);
        public abstract RepositoryModel GetRepository(string Name, StringComparison compType = StringComparison.OrdinalIgnoreCase);
        public abstract bool Create(RepositoryModel repository);
        public abstract void Update(RepositoryModel repository);
        public abstract void Delete(Guid id);
        public abstract IList<RepositoryModel> GetAllRepositories();
    }
}