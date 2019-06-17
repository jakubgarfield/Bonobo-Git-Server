using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bonobo.Git.Server.Extensions
{
    public static class DatabaseFacadeExtensions
    {
        public static object ExecuteScalar(this DatabaseFacade facade, string command)
        {
            using (var conn = facade.GetDbConnection().CreateCommand())
            {
                conn.CommandText = command;
                conn.CommandType = CommandType.Text;

                facade.OpenConnection();
                return conn.ExecuteScalar();
            }
        }
    }
}
