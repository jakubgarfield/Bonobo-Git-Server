using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading;
using System.Web;

using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;

using Microsoft.Practices.Unity;
using System.Threading.Tasks;

namespace Bonobo.Git.Server.Data
{
    public class ADTeamRepository : ITeamRepository
    {

        Dictionary<int, string> _id_to_name = new Dictionary<int, string>();

        public bool Create(TeamModel team)
        {
            throw new NotImplementedException();
        }

        public void Delete(string name)
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

        public TeamModel GetTeam(int id)
        {
            var name = _id_to_name[id];
            return GetTeam(name);
        }

        public TeamModel GetTeam(string name)
        {
            return ADBackend.Instance.Teams[name];
        }

        public IList<TeamModel> GetTeams(string username)
        {
            return ADBackend.Instance.Teams.Where(x => x.Members.Select(y => y.Name).Contains(username, StringComparer.OrdinalIgnoreCase)).ToList();
        }

        public void Update(TeamModel team)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserTeams(string userName, List<string> newTeams)
        {
            throw new NotImplementedException();
        }
    }
}