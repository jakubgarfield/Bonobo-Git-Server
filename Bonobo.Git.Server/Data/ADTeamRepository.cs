using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Data
{
    public class ADTeamRepository : ITeamRepository
    {
        Dictionary<Guid, string> _id_to_name = new Dictionary<Guid, string>();
        private readonly ADBackend _adBackend;

        public ADTeamRepository(ADBackend adBackend)
        {
            _adBackend = adBackend;
        }

        public bool Create(TeamModel team)
        {
            throw new NotSupportedException();
        }

        public void Delete(Guid name)
        {
            throw new NotSupportedException();
        }

        public IList<TeamModel> GetAllTeams()
        {
            var ret = _adBackend.Teams.ToList();
            foreach (var t in ret)
            {
                _id_to_name[t.Id] = t.Name;
            }
            return ret;
        }

        public TeamModel GetTeam(Guid TeamId)
        {
            return _adBackend.Teams[TeamId];
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
            return _adBackend.Teams.Where(x => x.Members.Any(y => y.Id == userId)).ToList();
        }

        public void Update(TeamModel team)
        {
            throw new NotSupportedException();
        }

        public void UpdateUserTeams(Guid userId, List<string> newTeams)
        {
            throw new NotSupportedException();
        }
    }
}