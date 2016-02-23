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
            repository.Id = Guid.NewGuid();

            return ADBackend.Instance.Repositories.Add(SanitizeModel(repository));
        }

        public void Delete(Guid Id)
        {
            ADBackend.Instance.Repositories.Remove(Id);
        }

        public IList<RepositoryModel> GetAdministratedRepositories(Guid userId)
        {
            return ADBackend.Instance.Repositories.Where(x => x.Administrators.Any(y => y.Id == userId)).ToList();
        }

        public IList<RepositoryModel> GetAllRepositories()
        {
            return ADBackend.Instance.Repositories.ToList();
        }

        public IList<RepositoryModel> GetPermittedRepositories(Guid userId, Guid[] userTeamsId)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Do not pass invalid userId", "userId");
            return GetAllRepositories().Where(x =>
                x.Users.Any(user => user.Id == userId) ||
                x.Teams.Any(team => userTeamsId.Contains(team.Id))
                ).ToList();
        }

        public IList<RepositoryModel> GetTeamRepositories(Guid[] teamsId)
        {
            return GetAllRepositories().Where(repo => repo.Teams.Any(team => teamsId.Contains(team.Id))).ToList();
        }

        public RepositoryModel GetRepository(string name)
        {
            return ADBackend.Instance.Repositories.FirstOrDefault(o => o.Name == name);
        }
        
        public RepositoryModel GetRepository(Guid id)
        {
            return ADBackend.Instance.Repositories[id];
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
 