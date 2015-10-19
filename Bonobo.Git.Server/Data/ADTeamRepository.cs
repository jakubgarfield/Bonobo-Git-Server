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
            return ADBackend.Instance.Teams.ToList();
        }

        public TeamModel GetTeam(string name)
        {
            return ADBackend.Instance.Teams[name];
        }

        public IList<TeamModel> GetTeams(string username)
        {
            return ADBackend.Instance.Teams.Where(x => x.Members.Contains(username, StringComparer.OrdinalIgnoreCase)).ToList();
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