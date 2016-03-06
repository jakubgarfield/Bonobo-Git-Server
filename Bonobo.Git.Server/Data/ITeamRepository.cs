using System.Collections.Generic;
using Bonobo.Git.Server.Models;
using System;

namespace Bonobo.Git.Server.Data
{
    public interface ITeamRepository
    {
        IList<TeamModel> GetAllTeams();
        IList<TeamModel> GetTeams(Guid userId);
        TeamModel GetTeam(Guid id);
        TeamModel GetTeam(string name, StringComparison compType = StringComparison.OrdinalIgnoreCase);
        void Delete(Guid Id);
        bool Create(TeamModel team);
        void Update(TeamModel team);
        void UpdateUserTeams(Guid userId, List<string> newTeams);
    }
}