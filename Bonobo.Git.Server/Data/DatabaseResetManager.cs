using System;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
        private readonly IConfiguration configuration;
        private readonly IOptions<AppSettings> _appSettings;

        public IRepositoryRepository Repository { get; set; }
        public IRoleProvider RoleProvider { get; set; }
        public ITeamRepository TeamRepository { get; set; }
        public IMembershipService Users { get; set; }

        public DatabaseResetManager(
            IRepositoryRepository Repository,
            IRoleProvider RoleProvider,
            ITeamRepository TeamRepository,
            IMembershipService Users,
            IConfiguration configuration,
            IOptions<AppSettings> appSettings)
        {
            this.Repository = Repository;
            this.RoleProvider = RoleProvider;
            this.TeamRepository = TeamRepository;
            this.Users = Users;
            this.configuration = configuration;
            this._appSettings = appSettings;
        }

        public void DoReset(int mode)
        {
            Log.Information("Reset mode {mode}", mode);
            Log.Information("AppSettings Allow: {AllowDBReset}", _appSettings.Value.AllowDBReset);
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
