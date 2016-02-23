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
        IList<RepositoryModel> GetPermittedRepositories(Guid userId, Guid[] userTeamsId);
        IList<RepositoryModel> GetTeamRepositories(Guid[] teamsId);
        IList<RepositoryModel> GetAdministratedRepositories(Guid userId);
        RepositoryModel GetRepository(Guid id);
        RepositoryModel GetRepository(string Name);
        bool Create(RepositoryModel repository);
        void Update(RepositoryModel repository);
        void Delete(Guid id);
    }
}