using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Data
{
    public class ADTeamRepository : ITeamRepository
    {
        Dictionary<Guid, string> _id_to_name = new Dictionary<Guid, string>();

        public bool Create(TeamModel team)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid name)
        {
            throw new NotImplementedException();
        }

        public IList<TeamModel> GetAllTeams()
        {
            var ret = ADBackend.Instance.Teams.ToList();
            foreach (var t in ret)
            {
                _id_to_name[t.Id] = t.Name;
            }
            return ret;
        }

        public TeamModel GetTeam(Guid TeamId)
        {
            return ADBackend.Instance.Teams[TeamId];
        }

        public TeamModel GetTeam(string name)
        {
            var teams = GetAllTeams();
            foreach (var team in teams)
            {
                if (name.Equals(team.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return team;
                }
            }
            return null;
        }

        public IList<TeamModel> GetTeams(Guid userId)
        {
            return ADBackend.Instance.Teams.Where(x => x.Members.Any(y => y.Id == userId)).ToList();
        }

        public void Update(TeamModel team)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserTeams(Guid userId, List<string> newTeams)
        {
            throw new NotImplementedException();
        }
    }
}