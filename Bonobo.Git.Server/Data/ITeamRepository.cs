using System.Collections.Generic;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Data
{
    public interface ITeamRepository
    {
        IList<TeamModel> GetAllTeams();
        IList<TeamModel> GetTeams(string username);
        TeamModel GetTeam(string name);
        void Delete(string name);
        bool Create(TeamModel team);
        void Update(TeamModel team);
        void UpdateUserTeams(string userName, List<string> newTeams);
    }
}