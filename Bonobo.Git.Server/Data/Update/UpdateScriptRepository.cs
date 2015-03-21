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
                        new InitialCreateScript(),
                        new InsertDefaultData(),
                        new UsernamesToLower(),
                        new AddAuditPushUser(),
                        new AddGroup()
                    };
                case "SqlConnection":
                    return new List<IUpdateScript>
                    {
                        new SqlServer.InitialCreateScript(),
                        new SqlServer.InsertDefaultData(),
                        new UsernamesToLower(),
                        new SqlServer.AddAuditPushUser(),
                        new SqlServer.AddGroup()
                    };
                default:
                    throw new NotImplementedException(string.Format("The provider '{0}' is not supported yet", sqlProvider));
            }
        }
    }
}