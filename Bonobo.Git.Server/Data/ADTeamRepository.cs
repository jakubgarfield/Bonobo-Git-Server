﻿using System;
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

        public TeamModel GetTeam(string name, StringComparison compType)
        {
            var teams = GetAllTeams();
            foreach (var team in teams)
            {
                if (name.Equals(name, compType))
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