using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Data
{
    public class ADRepositoryRepository : IRepositoryRepository
    {
        public bool Create(RepositoryModel repository)
        {
            // Make sure we don't already have a repo with this name
            if (GetRepository(repository.Name) != null)
            {
                return false;
            }

            repository.Id = Guid.NewGuid();
            return ADBackend.Instance.Repositories.Add(SanitizeModel(repository));
        }

        public void Delete(Guid id)
        {
            ADBackend.Instance.Repositories.Remove(id);
        }

        public IList<RepositoryModel> GetAllRepositories()
        {
            return ADBackend.Instance.Repositories.ToList();
        }

        public RepositoryModel GetRepository(string name, StringComparison compType = StringComparison.OrdinalIgnoreCase)
        {
            var repos = GetAllRepositories();
            foreach (var repo in repos)
            {
                if (repo.Name.Equals(name, compType))
                {
                    return repo;
                }
            }
            return null;
        }
        
        public RepositoryModel GetRepository(Guid id)
        {
            var result = ADBackend.Instance.Repositories[id];
            if (result == null)
            {
                // Ensure that we behave the same way as the EF reporepo
                throw new InvalidOperationException("Cannot find repository with ID " + id);
            }
            return result;
        }

        public void Update(RepositoryModel repository)
        {
            if (repository.RemoveLogo)
            {
                repository.Logo = null;
            }
            else if (repository.Logo == null)
            {
                // If we're given a null logo, then we need to preserve the existing one
                repository.Logo = GetRepository(repository.Id).Logo;
            }
            ADBackend.Instance.Repositories.Update(SanitizeModel(repository));
        }

        private static RepositoryModel SanitizeModel(RepositoryModel model)
        {
            model.EnsureCollectionsAreValid();
            return model;
        }

        public IList<RepositoryModel> GetTeamRepositories(Guid[] teamsId)
        {
            return GetAllRepositories().Where(repo => repo.Teams.Any(team => teamsId.Contains(team.Id))).ToList();
        }
    }
}
 
