using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bonobo.Git.Server.Models;
using System.IO;
using System.Configuration;

using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Concurrent;

namespace Bonobo.Git.Server.Data
{
    public class ADRepositoryRepository : IRepositoryRepository
    {
        Dictionary<int, string> _id_to_name = new Dictionary<int, string>();

        public bool Create(RepositoryModel repository)
        {
            // To populate _id_to_name table
            GetAllRepositories();
            repository.Id = _id_to_name.Keys.Count + 1;
            _id_to_name[repository.Id] = repository.Name;
            return ADBackend.Instance.Repositories.Add(SanitizeModel(repository));
        }

        public void Delete(string name)
        {
            var repo = GetRepository(name);
            _id_to_name.Remove(repo.Id);
            ADBackend.Instance.Repositories.Remove(name);
        }

        public IList<RepositoryModel> GetAdministratedRepositories(string username)
        {
            return ADBackend.Instance.Repositories.Where(x => x.Administrators.Select(y => y.Name).Contains(username, StringComparer.OrdinalIgnoreCase)).ToList();
        }

        public IList<RepositoryModel> GetAllRepositories()
        {
            var repos = ADBackend.Instance.Repositories.ToList();
            foreach(var repo in repos)
            {
                _id_to_name[repo.Id] = repo.Name;
            }
            return repos;
        }

        public IList<RepositoryModel> GetPermittedRepositories(string username, string[] userTeams)
        {
            return ADBackend.Instance.Repositories.Where(x => 
                (String.IsNullOrEmpty(username) ? false : x.Users.Select(y => y.Name).Contains(username, StringComparer.OrdinalIgnoreCase)) ||
                x.Teams.Any(s => userTeams.Contains(s.Name, StringComparer.OrdinalIgnoreCase))
                ).ToList();
        }

        public RepositoryModel GetRepository(string name)
        {
            return ADBackend.Instance.Repositories[name]; 
        }

        public RepositoryModel GetRepository(int id)
        {
            GetAllRepositories();
            var name = _id_to_name[id];
            return GetRepository(name);
        }

        public void Update(RepositoryModel repository)
        {
            _id_to_name[repository.Id] = repository.Name;
            ADBackend.Instance.Repositories.Update(SanitizeModel(repository));
        }

        private RepositoryModel SanitizeModel(RepositoryModel model)
        {
            if (model.Administrators == null)
            {
                model.Administrators = new UserModel[0];
            }

            if (model.Users == null)
            {
                model.Users = new UserModel[0];
            }

            if (model.Teams == null)
            {
                model.Teams = new TeamModel[0];
            }

            return model;
        }
    }
}
 