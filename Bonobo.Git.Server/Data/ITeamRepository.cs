using System.Collections.Generic;
using Bonobo.Git.Server.Models;
using System;

namespace Bonobo.Git.Server.Data
{
    public interface ITeamRepository
    {
        IList<TeamModel> GetAllTeams();
        IList<TeamModel> GetTeams(Guid userId);
        IList<TeamModel> GetTeams(string userName);
        TeamModel GetTeam(Guid id);
        void Delete(Guid Id);
        bool Create(TeamModel team);
        void Update(TeamModel team);
        void UpdateUserTeams(string userName, List<string> newTeams);
    }
}