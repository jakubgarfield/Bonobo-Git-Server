using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.Models;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using Microsoft.Practices.Unity;

namespace Bonobo.Git.Server.Data
{
    public class EFRepositoryRepository : IRepositoryRepository
    {
        [Dependency]
        public Func<BonoboGitServerContext> CreateContext { get; set; }

        public IList<RepositoryModel> GetAllRepositories()
        {
            using (var db = CreateContext())
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
                    AllowAnonPush = repo.AllowAnonymousPush,
                    Logo = repo.Logo
                }).ToList();

                return dbrepos.Select(repo => new RepositoryModel
                {
                    Id = repo.Id,
                    Name = repo.Name,
                    Group = repo.Group,
                    Description = repo.Description,
                    AnonymousAccess = repo.AnonymousAccess,
                    Users = repo.Users.Select(user => user.ToModel()).ToArray(),
                    Teams = repo.Teams.Select(TeamToTeamModel).ToArray(),
                    Administrators = repo.Administrators.Select(user => user.ToModel()).ToArray(),
                    AuditPushUser = repo.AuditPushUser,
                    AllowAnonymousPush = repo.AllowAnonPush,
                    Logo = repo.Logo
                }).ToList();
            }
        }

        public RepositoryModel GetRepository(string name, StringComparison compType)
        {
            if (name == null) throw new ArgumentNullException("name");

            /* The straight-forward solution of using FindFirstOrDefault with
             * string.Equal does not work. Even name.Equals with OrdinalIgnoreCase does not
             * as it seems to get translated into some specific SQL syntax and EF does not
             * provide case insensitive matching :( */
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
            using (var db = CreateContext())
            {
                return ConvertToModel(db.Repositories.First(i => i.Id.Equals(id)));
            }
        }

        public void Delete(Guid id)
        {
            using (var db = CreateContext())
            {
                var repo = db.Repositories.FirstOrDefault(i => i.Id == id);
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

            using (var database = CreateContext())
            {
                model.EnsureCollectionsAreValid();
                model.Id = Guid.NewGuid();
                var repository = new Repository
                {
                    Id = model.Id,
                    Name = model.Name,
                    Logo = model.Logo,
                    Group = model.Group,
                    Description = model.Description,
                    Anonymous = model.AnonymousAccess,
                    AllowAnonymousPush = model.AllowAnonymousPush,
                    AuditPushUser = model.AuditPushUser,
                    LinksUseGlobal = model.LinksUseGlobal,
                    LinksUrl = model.LinksUrl,
                    LinksRegex = model.LinksRegex 
                };
                database.Repositories.Add(repository);
                AddMembers(model.Users.Select(x => x.Id), model.Administrators.Select(x => x.Id), model.Teams.Select(x => x.Id), repository, database);
                try
                {
                    database.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    Trace.TraceWarning("Failed to create repo {0} - {1}", model.Name, ex);
                    return false;
                }
                catch (UpdateException)
                {
                    return false;
                }
                return true;
            }
        }

        public void Update(RepositoryModel model)
        {
            if (model == null) throw new ArgumentException("model");
            if (model.Name == null) throw new ArgumentException("name");

            using (var db = CreateContext())
            {
                var repo = db.Repositories.FirstOrDefault(i => i.Id == model.Id);
                if (repo != null)
                {
                    model.EnsureCollectionsAreValid();

                    repo.Name = model.Name;
                    repo.Group = model.Group;
                    repo.Description = model.Description;
                    repo.Anonymous = model.AnonymousAccess;
                    repo.AuditPushUser = model.AuditPushUser;
                    repo.AllowAnonymousPush = model.AllowAnonymousPush;
                    repo.LinksRegex = model.LinksRegex;
                    repo.LinksUrl = model.LinksUrl;
                    repo.LinksUseGlobal = model.LinksUseGlobal;

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

        private TeamModel TeamToTeamModel(Team t)
        {
            return new TeamModel
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Members = t.Users.Select(user => user.ToModel()).ToArray()
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
                Users = item.Users.Select(user => user.ToModel()).ToArray(),
                Teams = item.Teams.Select(TeamToTeamModel).ToArray(),
                Administrators = item.Administrators.Select(user => user.ToModel()).ToArray(),
                AuditPushUser = item.AuditPushUser,
                AllowAnonymousPush = item.AllowAnonymousPush,
                Logo = item.Logo,
                LinksRegex = item.LinksRegex,
                LinksUrl = item.LinksUrl,
                LinksUseGlobal = item.LinksUseGlobal

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

        public IList<RepositoryModel> GetTeamRepositories(Guid[] teamsId)
        {
            return GetAllRepositories().Where(repo => repo.Teams.Any(team => teamsId.Contains(team.Id))).ToList();
        }
    }
}
