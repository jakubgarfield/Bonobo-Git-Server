using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.Data.Mapping;
using Bonobo.Git.Server.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Bonobo.Git.Server.Data
{
    public class EFRepositoryRepository : IRepositoryRepository
    {
        private BonoboGitServerContext _ctx;
        public EFRepositoryRepository(BonoboGitServerContext createContext)
        {
            _ctx = createContext;
        }

        public BonoboGitServerContext CreateContext() => _ctx;

        public IList<RepositoryModel> GetAllRepositories()
        {
            var db = CreateContext();
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
                    Users = repo.Users.Select(user => user.User.ToModel()).ToArray(),
                    Teams = repo.Teams.Select(t => TeamToTeamModel(t.Team)).ToArray(),
                    Administrators = repo.Administrators.Select(user => user.User.ToModel()).ToArray(),
                    AuditPushUser = repo.AuditPushUser,
                    AllowAnonymousPush = repo.AllowAnonPush,
                    Logo = repo.Logo
                }).ToList();
            }
        }

        public RepositoryModel GetRepository(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            /* The straight-forward solution of using FindFirstOrDefault with
             * string.Equal does not work. Even name.Equals with OrdinalIgnoreCase does not
             * as it seems to get translated into some specific SQL syntax and EF does not
             * provide case insensitive matching :( */
            var repos = GetAllRepositories();
            foreach (var repo in repos)
            {
                if (repo.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return repo;
                }
            }
            return null;
        }

        public RepositoryModel GetRepository(Guid id)
        {
            var db = CreateContext();
            {
                return ConvertToModel(db.Repositories.First(i => i.Id.Equals(id)));
            }
        }

        public void Delete(Guid id)
        {
            var db = CreateContext();
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

        public bool NameIsUnique(string newName, Guid ignoreRepoId)
        {
            var repo = GetRepository(newName);
            return repo == null || repo.Id == ignoreRepoId;
        }

        public bool Create(RepositoryModel model)
        {
            if (model == null) throw new ArgumentException("model");
            if (model.Name == null) throw new ArgumentException("name");

            var database = CreateContext();
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
                    Log.Error(ex, "Failed to create repo {RepoName}", model.Name);
                    return false;
                }
                //catch (UpdateException)
                //{
                //    return false;
                //}
                return true;
            }
        }

        public void Update(RepositoryModel model)
        {
            if (model == null) throw new ArgumentException("model");
            if (model.Name == null) throw new ArgumentException("name");

            var db = CreateContext();
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
                Members = t.Users.Select(user => user.User.ToModel()).ToArray()
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
                Users = item.Users.Select(user => user.User.ToModel()).ToArray(),
                Teams = item.Teams.Select(t => TeamToTeamModel(t.Team)).ToArray(),
                Administrators = item.Administrators.Select(user => user.User.ToModel()).ToArray(),
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
                    var u = new UserRepositoryAdministrator
                    {
                        RepositoryId = repo.Id,
                        UserId = item.Id,
                    };
                    repo.Administrators.Add(u);
                }
            }

            if (users != null)
            {
                var permittedUsers = database.Users.Where(i => users.Contains(i.Id));
                foreach (var item in permittedUsers)
                {
                    var u = new UserRepositoryPermission
                    {
                        UserId = item.Id,
                        RepositoryId = repo.Id,
                    };
                    repo.Users.Add(u);
                }
            }

            if (teams != null)
            {
                var permittedTeams = database.Teams.Where(i => teams.Contains(i.Id));
                foreach (var item in permittedTeams)
                {
                    var u = new TeamRepositoryPermission
                    {
                        RepositoryId = repo.Id,
                        TeamId = item.Id,
                    };
                    repo.Teams.Add(u);
                }
            }
        }

        public IList<RepositoryModel> GetTeamRepositories(Guid[] teamsId)
        {
            return GetAllRepositories().Where(repo => repo.Teams.Any(team => teamsId.Contains(team.Id))).ToList();
        }
    }
}
