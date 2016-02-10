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
        public bool Create(RepositoryModel repository)
        {
            // To populate _id_to_name table
            GetAllRepositories();
            repository.Id = Guid.NewGuid();

            return ADBackend.Instance.Repositories.Add(SanitizeModel(repository));
        }

        public void Delete(Guid Id)
        {
            ADBackend.Instance.Repositories.Remove(Id.ToString());
        }

        public IList<RepositoryModel> GetAdministratedRepositories(Guid Id)
        {
            return ADBackend.Instance.Repositories.Where(x => x.Administrators.Any(y => y.Id == Id)).ToList();
        }

        public IList<RepositoryModel> GetAllRepositories()
        {
            return ADBackend.Instance.Repositories.ToList();
        }

        public IList<RepositoryModel> GetPermittedRepositories(Guid? userId, Guid[] userTeamsId)
        {
            return ADBackend.Instance.Repositories.Where(x => 
                (userId != null ? false : x.Users.Count(y => y.Id == userId) > 0) ||
                x.Teams.Any(s => userTeamsId.Contains(userId.Value))
                ).ToList();
        }

        public RepositoryModel GetRepository(string name)
        {
            return ADBackend.Instance.Repositories.FirstOrDefault(o => o.Name == name);
        }
        
        public RepositoryModel GetRepository(Guid id)
        {
            return ADBackend.Instance.Repositories[id.ToString()];
        }

        public void Update(RepositoryModel repository)
        {
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
 