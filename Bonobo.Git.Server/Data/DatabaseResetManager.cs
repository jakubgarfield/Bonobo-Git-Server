﻿using Bonobo.Git.Server.Security;
using Serilog;
using System;
using System.Configuration;
using Unity;

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
        [Dependency]
        public IRepositoryRepository Repository { get; set; }

        [Dependency]
        public IRoleProvider RoleProvider { get; set; }

        [Dependency]
        public ITeamRepository TeamRepository { get; set; }

        [Dependency]
        public IMembershipService Users { get; set; }

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
