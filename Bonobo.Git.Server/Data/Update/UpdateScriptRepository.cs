using System;
using System.Collections.Generic;
using Bonobo.Git.Server.Security;

namespace Bonobo.Git.Server.Data.Update
{
    public static class UpdateScriptRepository
    {
        /// <summary>
        /// Creates the list of scripts that should be executed on app start. Ordering matters!
        /// </summary>
        public static IEnumerable<IUpdateScript> GetScriptsBySqlProviderName(string sqlProvider, IAuthenticationProvider authenticationProvider)
        {
            switch (sqlProvider)
            {
                case "Microsoft.EntityFrameworkCore.Sqlite":
                    return new List<IUpdateScript>
                    {
                        new Sqlite.InitialCreateScript(),
                        new UsernamesToLower(),
                        new Sqlite.AddAuditPushUser(),
                        new Sqlite.AddGroup(),
                        new Sqlite.AddRepositoryLogo(),
                        new Sqlite.AddGuidColumn(authenticationProvider),
                        new Sqlite.AddRepoPushColumn(),
                        new Sqlite.AddRepoLinksColumn(),
                        new Sqlite.InsertDefaultData()
                    };
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    return new List<IUpdateScript>
                    {
                        new SqlServer.InitialCreateScript(),
                        new UsernamesToLower(),
                        new SqlServer.AddAuditPushUser(),
                        new SqlServer.AddGroup(),
                        new SqlServer.AddRepositoryLogo(),
                        new SqlServer.AddGuidColumn(authenticationProvider),
                        new SqlServer.AddRepoPushColumn(),
                        new SqlServer.AddRepoLinksColumn(),
                        new SqlServer.InsertDefaultData()
                    };
                default:
                    throw new NotImplementedException($"The provider '{sqlProvider}' is not supported yet");
            }
        }
    }
}
