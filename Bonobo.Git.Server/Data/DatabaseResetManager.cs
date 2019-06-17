using System;
using Bonobo.Git.Server.Security;
using System.Configuration;
using Serilog;

namespace Bonobo.Git.Server.Data
{
    public interface IDatabaseResetManager
    {
        void DoReset(int mode);
    }

    /// <summary>
    /// Provide reset services, to allow the database to be set to a known state
    /// </summary>
    public class DatabaseResetManager : IDatabaseResetManager
    {
        public IRepositoryRepository Repository { get; set; }

        public IRoleProvider RoleProvider { get; set; }

        public ITeamRepository TeamRepository { get; set; }

        public IMembershipService Users { get; set; }

        public DatabaseResetManager(IRepositoryRepository repositoryRepository, IRoleProvider roleProvider,
            ITeamRepository teamRepository, IMembershipService users)
        {
            Repository = repositoryRepository;
            RoleProvider = roleProvider;
            TeamRepository = teamRepository;
            Users = users;
        }

        public void DoReset(int mode)
        {
            Log.Information("Reset mode {mode}", mode);
            Log.Information("AppSettings Allow: {AllowDBReset}", ConfigurationManager.AppSettings["AllowDBReset"]);
            switch (mode)
            {
                case 1:
                    DoFullReset();
                    break;

                default:
                    throw new ArgumentException("Requested invalid reset mode " + mode.ToString());
            }
        }

        /// <summary>
        /// Clear out everything except the admin user
        /// </summary>
        private void DoFullReset()
        {
            foreach (var repository in Repository.GetAllRepositories())
            {
                Repository.Delete(repository.Id);
            }
            foreach (var team in TeamRepository.GetAllTeams())
            {
                TeamRepository.Delete(team.Id);
            }
            foreach (var user in Users.GetAllUsers())
            {
                if (!user.Username.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    Users.DeleteUser(user.Id);
                }
            }
            foreach (var role in RoleProvider.GetAllRoles())
            {
                if (role != Definitions.Roles.Administrator)
                {
                    RoleProvider.DeleteRole(role, true);
                }
            }
        }
    }
}
