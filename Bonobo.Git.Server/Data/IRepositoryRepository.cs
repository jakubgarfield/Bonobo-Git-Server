using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Data
{
    public interface IRepositoryRepository
    {
        IList<RepositoryModel> GetAllRepositories();
        IList<RepositoryModel> GetTeamRepositories(Guid[] teamsId);
        RepositoryModel GetRepository(Guid id);
        RepositoryModel GetRepository(string Name, StringComparison compType = StringComparison.OrdinalIgnoreCase);
        bool Create(RepositoryModel repository);
        void Update(RepositoryModel repository);
        void Delete(Guid id);
    }
}