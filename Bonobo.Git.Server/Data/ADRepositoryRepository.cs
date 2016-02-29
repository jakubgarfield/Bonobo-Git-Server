using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Data
{
    public class ADRepositoryRepository : RepositoryRepositoryBase
    {
        public override bool Create(RepositoryModel repository)
        {
            // Make sure we don't already have a repo with this name
            if (GetRepository(repository.Name) != null)
            {
                return false;
            }

            repository.Id = Guid.NewGuid();
            return ADBackend.Instance.Repositories.Add(SanitizeModel(repository));
        }

        public override void Delete(Guid id)
        {
            ADBackend.Instance.Repositories.Remove(id);
        }

        public override IList<RepositoryModel> GetAllRepositories()
        {
            return ADBackend.Instance.Repositories.ToList();
        }

        public override RepositoryModel GetRepository(string name)
        {
            return ADBackend.Instance.Repositories.FirstOrDefault(o => o.Name == name);
        }
        
        public override RepositoryModel GetRepository(Guid id)
        {
            var result = ADBackend.Instance.Repositories[id];
            if (result == null)
            {
                // Ensure that we behave the same way as the EF reporepo
                throw new InvalidOperationException("Cannot find repository with ID " + id);
            }
            return result;
        }

        public override void Update(RepositoryModel repository)
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
    }
}
 