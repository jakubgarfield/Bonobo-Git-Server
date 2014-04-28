using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Data.Update
{
    public class RepositorySynchronizer
    {
        IRepositoryRepository _repositoryRepository = DependencyResolver.Current.GetService<IRepositoryRepository>();

        public virtual void Run()
        {
            CheckForNewRepositories();
        }

        private void CheckForNewRepositories()
        {
            IEnumerable<string> directories = Directory.EnumerateDirectories(UserConfiguration.Current.Repositories);
            foreach (string directory in directories)
            {
                string name = Path.GetFileNameWithoutExtension(directory);
                string gitName = Path.GetFileName(directory);

                RepositoryModel repository = _repositoryRepository.GetRepository(name);
                if (repository == null)
                {
                    repository = new RepositoryModel();
                    repository.Description = "Discovered in file system.";
                    repository.Name = name;
                    repository.GitName = gitName;
                    repository.AnonymousAccess = false;
                    _repositoryRepository.Create(repository);
                }
            }
        }
    }
}
