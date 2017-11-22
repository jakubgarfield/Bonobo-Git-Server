using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Data
{
    public class ADRepositoryRepository : IRepositoryRepository
    {
        private readonly ADBackend _adBackend;

        public ADRepositoryRepository(ADBackend adBackend)
        {
            this._adBackend = adBackend;
        }
        public bool Create(RepositoryModel repository)
        {
            // Make sure we don't already have a repo with this name
            if (GetRepository(repository.Name) != null)
            {
                return false;
            }

            repository.Id = Guid.NewGuid();
            return _adBackend.Repositories.Add(SanitizeModel(repository));
        }

        public void Delete(Guid id)
        {
            _adBackend.Repositories.Remove(id);
        }

        public bool NameIsUnique(string newName, Guid ignoreRepoId)
        {
            var existingRepo = GetRepository(newName);
            return existingRepo == null || existingRepo.Id == ignoreRepoId;
        }

        public IList<RepositoryModel> GetAllRepositories()
        {
            return _adBackend.Repositories.ToList();
        }

        public RepositoryModel GetRepository(string name)
        {
            return
                _adBackend.Repositories.FirstOrDefault(
                    repo => repo.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public RepositoryModel GetRepository(Guid id)
        {
            var result = _adBackend.Repositories[id];
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
            _adBackend.Repositories.Update(SanitizeModel(repository));
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

