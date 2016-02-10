using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.Models;
using System.Data;
using System.Data.Entity.Core;

namespace Bonobo.Git.Server.Data
{
    public class EFRepositoryRepository : IRepositoryRepository
    {
        public IList<RepositoryModel> GetAllRepositories()
        {
            using (var db = new BonoboGitServerContext())
            {
                var dbrepos = db.Repositories.Select(repo => new
                {
                    Id = repo.Id,
                    Name = repo.Name,
                    Group = repo.Group,
                    Description = repo.Description,
                    AnonymousAccess = repo.Anonymous,
                    Users = repo.Users,
                    Teams = repo.Teams,
                    Administrators = repo.Administrators,
                    AuditPushUser = repo.AuditPushUser,
                    Logo = repo.Logo
                }).ToList();

                return dbrepos.Select(repo => new RepositoryModel
                {
                    Id = repo.Id,
                    Name = repo.Name,
                    Group = repo.Group,
                    Description = repo.Description,
                    AnonymousAccess = repo.AnonymousAccess,
                    Users = repo.Users.Select(UserToUserModel).ToArray(),
                    Teams = repo.Teams.Select(TeamToTeamModel).ToArray(),
                    Administrators = repo.Administrators.Select(UserToUserModel).ToArray(),
                    AuditPushUser = repo.AuditPushUser,
                    Logo = repo.Logo
                }).ToList();
            }
        }

        public IList<RepositoryModel> GetPermittedRepositories(Guid? UserId, Guid[] teamsId)
        {
            return GetAllRepositories().Where( i => 
                i.Users.Any(x => x.Id == UserId) ||
                i.Administrators.Any(x => x.Id == UserId) ||
                i.Teams.FirstOrDefault(t => teamsId.Contains(t.Id)) != null ||
                i.AnonymousAccess).ToList();
        }

        public IList<RepositoryModel> GetAdministratedRepositories(Guid UserId)
        {
            if (UserId == null) throw new ArgumentException("UserId");
            
            return GetAllRepositories().Where(i => i.Administrators.Any(x => x.Id == UserId)).ToList();
        }

        public RepositoryModel GetRepository(string name)
        {
            if (name == null) throw new ArgumentException("name");

            using (var db = new BonoboGitServerContext())
            {
                return ConvertToModel(db.Repositories.FirstOrDefault(i => i.Name == name));
            }
        }

        public RepositoryModel GetRepository(Guid id)
        {
            using (var db = new BonoboGitServerContext())
            {
                return ConvertToModel(db.Repositories.Where(i => i.Id.Equals(id)).First());
            }
        }

        public void Delete(Guid RepositoryId)
        {
            using (var db = new BonoboGitServerContext())
            {
                var repo = db.Repositories.FirstOrDefault(i => i.Id == RepositoryId);
                if (repo != null)
                {
                    repo.Administrators.Clear();
                    repo.Users.Clear();
                    repo.Teams.Clear();
                    db.Repositories.Remove(repo);
                    db.SaveChanges();
                }
            }
        }

        public bool Create(RepositoryModel model)
        {
            if (model == null) throw new ArgumentException("model");
            if (model.Name == null) throw new ArgumentException("name");

            using (var database = new BonoboGitServerContext())
            {
                var repository = new Repository
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    Logo = model.Logo,
                    Group = model.Group,
                    Description = model.Description,
                    Anonymous = model.AnonymousAccess,
                    AuditPushUser = model.AuditPushUser,
                };
                database.Repositories.Add(repository);
                AddMembers(model.Users.Select(x => x.Id), model.Administrators.Select(x => x.Id), model.Teams.Select(x => x.Id), repository, database);
                try
                {
                    database.SaveChanges();
                }
                catch (UpdateException)
                {
                    return false;
                }
            }

            return true;
        }

        public void Update(RepositoryModel model)
        {
            if (model == null) throw new ArgumentException("model");
            if (model.Name == null) throw new ArgumentException("name");

            using (var db = new BonoboGitServerContext())
            {
                var repo = db.Repositories.FirstOrDefault(i => i.Id == model.Id);
                if (repo != null)
                {
                    repo.Group = model.Group;
                    repo.Description = model.Description;
                    repo.Anonymous = model.AnonymousAccess;
                    repo.AuditPushUser = model.AuditPushUser;

                    if (model.Logo != null)
                        repo.Logo = model.Logo;

                    if (model.RemoveLogo)
                        repo.Logo = null;

                    repo.Users.Clear();
                    repo.Teams.Clear();
                    repo.Administrators.Clear();

                    AddMembers(model.Users.Select(x => x.Id), model.Administrators.Select(x => x.Id), model.Teams.Select(x => x.Id), repo, db);

                    db.SaveChanges();
                }
            }
        }

        private UserModel UserToUserModel(User u)
        {
            return new UserModel
            {
                Id = u.Id,
                Name = u.Username,
                GivenName = u.Name,
                Surname = u.Surname,
                Email = u.Email,
            };
        }

        private TeamModel TeamToTeamModel(Team t)
        {
            return new TeamModel
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Members = t.Users.Select(UserToUserModel).ToArray()
            };
        }

        private RepositoryModel ConvertToModel(Repository item)
        {
            if (item == null)
            {
                return null;
            }

            return new RepositoryModel
            {
                Id = item.Id,
                Name = item.Name,
                Group = item.Group,
                Description = item.Description,
                AnonymousAccess = item.Anonymous,
                Users = item.Users.Select(UserToUserModel).ToArray(),
                Teams = item.Teams.Select(TeamToTeamModel).ToArray(),
                Administrators = item.Administrators.Select(UserToUserModel).ToArray(),
                AuditPushUser = item.AuditPushUser,
                Logo = item.Logo
            };
        }

        private void AddMembers(IEnumerable<Guid> users, IEnumerable<Guid> admins, IEnumerable<Guid> teams, Repository repo, BonoboGitServerContext database)
        {
            if (admins != null)
            {
                var administrators = database.Users.Where(i => admins.Contains(i.Id));
                foreach (var item in administrators)
                {
                    repo.Administrators.Add(item);
                }
            }

            if (users != null)
            {
                var permittedUsers = database.Users.Where(i => users.Contains(i.Id));
                foreach (var item in permittedUsers)
                {
                    repo.Users.Add(item);
                }
            }

            if (teams != null)
            {
                var permittedTeams = database.Teams.Where(i => teams.Contains(i.Id));
                foreach (var item in permittedTeams)
                {
                    repo.Teams.Add(item);
                }
            }
        }

    }
}