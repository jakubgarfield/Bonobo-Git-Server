using System;
using System.Collections.Generic;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Data
{
    public interface IRepositoryRepository
    {
        IList<RepositoryModel> GetAllRepositories();
        IList<RepositoryModel> GetTeamRepositories(Guid[] teamsId);
        RepositoryModel GetRepository(Guid id);
        RepositoryModel GetRepository(string Name);
        bool Create(RepositoryModel repository);
        void Update(RepositoryModel repository);
        void Delete(Guid id);
        bool NameIsUnique(string newName, Guid ignoreRepoId);
    }
}