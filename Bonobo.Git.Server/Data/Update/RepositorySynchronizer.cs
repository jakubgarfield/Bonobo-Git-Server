﻿using Bonobo.Git.Server.App_Start;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Models;
using LibGit2Sharp;
using Serilog;
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
            if (!Directory.Exists(UserConfiguration.Current.Repositories))
            {
                Log.Error($"Repo root doesn't exist: {UserConfiguration.Current.Repositories}");
                // We don't want an exception if the repo dir no longer exists, 
                // as this would make it impossible to start the server
                return;
            }
            IEnumerable<string> directories = Directory.EnumerateDirectories(UserConfiguration.Current.Repositories, "*.git", SearchOption.AllDirectories);
            foreach (string directory in directories)
            {
                var repoPath = directory.Remove(0, UserConfiguration.Current.Repositories.Length).TrimStart('\\');
                var rootDir = repoPath.Split('\\').FirstOrDefault();

                Log.Debug($"Repo {repoPath}");

                if (DoesControllerExistConstraint.DoesControllerExist(rootDir))
                    continue; //Do not load as a valid repo

                RepositoryModel repository = _repositoryRepository.GetRepository(repoPath);
                if (repository == null)
                {
                    if (LibGit2Sharp.Repository.IsValid(directory))
                    {
                        repository = new RepositoryModel();
                        repository.Id = Guid.NewGuid();
                        repository.Description = "Discovered in file system.";
                        repository.Name = repoPath;
                        repository.AnonymousAccess = false;
                        repository.Users = new UserModel[0];
                        repository.Teams = new TeamModel[0];
                        repository.Administrators = new UserModel[0];
                        if (repository.NameIsValid)
                        {
                            _repositoryRepository.Create(repository);
                        }
                    }
                }
            }
        }
    }
}
