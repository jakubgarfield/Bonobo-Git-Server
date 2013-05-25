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
        IList<RepositoryModel> GetPermittedRepositories(string username, string[] userTeams);
        IList<RepositoryModel> GetAdministratedRepositories(string username);
        RepositoryModel GetRepository(string name);
        void Delete(string name);
        bool Create(RepositoryModel repository);
        void Update(RepositoryModel repository);
    }
}