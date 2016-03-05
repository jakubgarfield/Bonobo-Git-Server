using System;
using System.Collections.Generic;

namespace Bonobo.Git.Server.Data.Update
{
    public static class UpdateScriptRepository
    {
        /// <summary>
        /// Creates the list of scripts that should be executed on app start. Ordering matters!
        /// </summary>
        public static IEnumerable<IUpdateScript> GetScriptsBySqlProviderName(string sqlProvider)
        {
            switch (sqlProvider)
            {
                case "SQLiteConnection":
                    return new List<IUpdateScript>
                    {
                        new Sqlite.InitialCreateScript(),
                        new Sqlite.InsertDefaultData(),
                        new UsernamesToLower(),
                        new Sqlite.AddAuditPushUser(),
                        new Sqlite.AddGroup(),
                        new Sqlite.AddRepositoryLogo(),
                        new Sqlite.AddGuidColumn(),
                        new Sqlite.AddRepoPushColumn(),
                        new Sqlite.AddRepoLinksColumn()
                    };
                case "SqlConnection":
                    return new List<IUpdateScript>
                    {
                        new SqlServer.InitialCreateScript(),
                        new SqlServer.InsertDefaultData(),
                        new UsernamesToLower(),
                        new SqlServer.AddAuditPushUser(),
                        new SqlServer.AddGroup(),
                        new SqlServer.AddRepositoryLogo(),
                        new SqlServer.AddGuidColumn(),
                        new SqlServer.AddRepoPushColumn(),
                        new SqlServer.AddRepoLinksColumn()
                    };
                default:
                    throw new NotImplementedException(string.Format("The provider '{0}' is not supported yet", sqlProvider));
            }
        }
    }
}
