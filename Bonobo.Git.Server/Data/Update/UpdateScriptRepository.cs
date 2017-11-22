using System;
using System.Collections.Generic;

namespace Bonobo.Git.Server.Data.Update
{
    public static class UpdateScriptRepository
    {
        /// <summary>
        /// Creates the list of scripts that should be executed on app start. Ordering matters!
        /// </summary>
        public static IEnumerable<IUpdateScript> GetScriptsBySqlProviderName(string sqlProvider, IServiceProvider serviceProvider)
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
                        new Sqlite.AddGuidColumn(serviceProvider),
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
                        new SqlServer.AddGuidColumn(serviceProvider),
                        new SqlServer.AddRepoPushColumn(),
                        new SqlServer.AddRepoLinksColumn(),
                        new SqlServer.InsertDefaultData()
                    };
                default:
                    throw new NotSupportedException($"The provider '{sqlProvider}' is not supported yet");
            }
        }
    }
}
