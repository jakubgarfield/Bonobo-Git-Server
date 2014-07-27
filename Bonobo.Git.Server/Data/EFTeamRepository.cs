using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Data
{
    public class EFTeamRepository : ITeamRepository
    {
        public IList<TeamModel> GetAllTeams()
        {
            using (var db = new BonoboGitServerContext())
            {
                var dbTeams = db.Teams.Select(team => new
                {
                    Name = team.Name,
                    Description = team.Description,
                    Members = team.Users.Select(i => i.Username),
                    Repositories = team.Repositories.Select(m => m.Name),
                }).ToList();

                return dbTeams.Select(item => new TeamModel
                {
                    Name = item.Name,
                    Description = item.Description,
                    Members = item.Members.ToArray(),
                    Repositories = item.Repositories.ToArray(),
                }).ToList();
            }
        }

        public IList<TeamModel> GetTeams(string username)
        {
            username = username.ToLowerInvariant(); 
            return GetAllTeams().Where(i => i.Members.Contains(username)).ToList();
        }

        public TeamModel GetTeam(string name)
        {
            if (name == null) throw new ArgumentException("name");

            using (var db = new BonoboGitServerContext())
            {
                var team = db.Teams.FirstOrDefault(i => i.Name == name);
                return team == null ? null : new TeamModel
                {
                    Name = team.Name,
                    Description = team.Description,
                    Members = team.Users.Select(m => m.Username).ToArray(),
                    Repositories = team.Repositories.Select(m => m.Name).ToArray(),
                };
            }
        }

        public void Delete(string name)
        {
            if (name == null) throw new ArgumentException("name");

            using (var db = new BonoboGitServerContext())
            {
                var team = db.Teams.FirstOrDefault(i => i.Name == name);
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

            using (var database = new BonoboGitServerContext())
            {
                var team = new Team
                {
                    Name = model.Name,
                    Description = model.Description
                };
                database.Teams.Add(team);
                if (model.Members != null)
                {
                    AddMembers(model.Members, team, database);
                }
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

        public void Update(TeamModel model)
        {
            if (model == null) throw new ArgumentException("team");
            if (model.Name == null) throw new ArgumentException("name");

            using (var db = new BonoboGitServerContext())
            {
                var team = db.Teams.FirstOrDefault(i => i.Name == model.Name);
                if (team != null)
                {
                    team.Description = model.Description;
                    team.Users.Clear();
                    if (model.Members != null)
                    {
                        AddMembers(model.Members, team, db);
                    }
                    db.SaveChanges();
                }
            }
        }

        private void AddMembers(IEnumerable<string> members, Team team, BonoboGitServerContext database)
        {
            var users = database.Users.Where(i => members.Contains(i.Username));
            foreach (var item in users)
            {
                team.Users.Add(item);
            }
        }

        public void UpdateUserTeams(string userName, List<string> newTeams)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentException("userName");
            if (newTeams == null) throw new ArgumentException("newTeams");

            using (var db = new BonoboGitServerContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Username == userName.ToLower());
                if (user != null)
                {
                    user.Teams.Clear();
                    var teams = db.Teams.Where(t => newTeams.Contains(t.Name));
                    foreach (var team in teams)
                    {
                        user.Teams.Add(team);
                    }
                    db.SaveChanges();
                }
            }
        }
    }
}