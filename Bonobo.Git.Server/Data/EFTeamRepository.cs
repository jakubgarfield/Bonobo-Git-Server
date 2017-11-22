﻿using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.Data.Mapping;
using Bonobo.Git.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Bonobo.Git.Server.Data
{
    public class EFTeamRepository : ITeamRepository
    {
        private BonoboGitServerContext _ctx;
        public EFTeamRepository(BonoboGitServerContext createContext)
        {
            _ctx = createContext;
        }

        public BonoboGitServerContext CreateContext() => _ctx;

        public IList<TeamModel> GetAllTeams()
        {
            var db = CreateContext();
            {
                var dbTeams = db.Teams.Select(team => new
                {
                    Id = team.Id,
                    Name = team.Name,
                    Description = team.Description,
                    Members = team.Users,
                    Repositories = team.Repositories.Select(m => m.Repository.Name),
                }).ToList();

                return dbTeams.Select(item => new TeamModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Members = item.Members.Select(user => user.User.ToModel()).ToArray(),
                }).ToList();
            }
        }

        public IList<TeamModel> GetTeams(Guid UserId)
        {
            return GetAllTeams().Where(i => i.Members.Any(x => x.Id == UserId)).ToList();
        }

        private TeamModel GetTeamModel(Team team)
        {
            return team == null ? null : new TeamModel
            {
                Id = team.Id,
                Name = team.Name,
                Description = team.Description,
                Members = team.Users.Select(user => user.User.ToModel()).ToArray(),
            };
        }

        public TeamModel GetTeam(Guid id)
        {
            var db = CreateContext();
            {
                var team = db.Teams.FirstOrDefault(i => i.Id == id);
                return GetTeamModel(team);
            }
        }

        public TeamModel GetTeam(string name)
        {
            var teams = GetAllTeams();
            foreach (var team in teams)
            {
                if (team.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return team;
                }
            }
            return null;
        }

        public void Delete(Guid teamId)
        {
            var db = CreateContext();
            {
                var team = db.Teams.FirstOrDefault(i => i.Id == teamId);
                if (team != null)
                {
                    team.Repositories.Clear();
                    team.Users.Clear();
                    db.Teams.Remove(team);
                    db.SaveChanges();
                }
            }
        }

        public bool Create(TeamModel model)
        {
            if (model == null) throw new ArgumentException("team");
            if (model.Name == null) throw new ArgumentException("name");

            var database = CreateContext();
            {
                // Write this into the model so that the caller knows the ID of the new itel
                model.Id = Guid.NewGuid();
                var team = new Team
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description
                };
                database.Teams.Add(team);
                if (model.Members != null)
                {
                    AddMembers(model.Members.Select(x => x.Id), team, database);
                }
                try
                {
                    database.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    return false;
                }
                //catch (UpdateException)
                //{
                //    // Not sure when this exception happens - DbUpdateException is what you get for adding a duplicate teamname
                //    return false;
                //}
            }

            return true;
        }

        public void Update(TeamModel model)
        {
            if (model == null) throw new ArgumentException("team");
            if (model.Name == null) throw new ArgumentException("name");

            var db = CreateContext();
            {
                var team = db.Teams.FirstOrDefault(i => i.Id == model.Id);
                if (team != null)
                {
                    team.Name = model.Name;
                    team.Description = model.Description;
                    team.Users.Clear();
                    if (model.Members != null)
                    {
                        AddMembers(model.Members.Select(x => x.Id), team, db);
                    }
                    db.SaveChanges();
                }
            }
        }

        private void AddMembers(IEnumerable<Guid> members, Team team, BonoboGitServerContext database)
        {
            var users = database.Users.Where(user => members.Contains(user.Id));
            foreach (var item in users)
            {
                var userTeamMember = new UserTeamMember()
                {
                    UserId = item.Id,
                    TeamId = team.Id,
                };
                team.Users.Add(userTeamMember);
            }
        }

        public void UpdateUserTeams(Guid userId, List<string> newTeams)
        {
            if (newTeams == null) throw new ArgumentException("newTeams");

            var db = CreateContext();
            {
                var user = db.Users.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    user.Teams.Clear();
                    var teams = db.Teams.Where(t => newTeams.Contains(t.Name));
                    foreach (var team in teams)
                    {
                        var userTeamMember = new UserTeamMember
                        {
                            TeamId = team.Id,
                            UserId = user.Id,
                        };
                        user.Teams.Add(userTeamMember);
                    }
                    db.SaveChanges();
                }
            }
        }
    }
}