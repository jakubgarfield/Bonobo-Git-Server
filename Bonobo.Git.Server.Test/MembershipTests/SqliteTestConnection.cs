using System;
using System.Data.Common;
using Bonobo.Git.Server.Data;

namespace Bonobo.Git.Server.Test.MembershipTests
{
    public interface IDatabaseTestConnection : IDisposable
    {
        BonoboGitServerContext GetContext();
    }

    class SqliteTestConnection : IDatabaseTestConnection
    {
        readonly DbConnection _connection;

        public SqliteTestConnection()
        {
            _connection = DbProviderFactories.GetFactory("System.Data.SQLite").CreateConnection();
            _connection.ConnectionString = "Data Source =:memory:;BinaryGUID=False";
            _connection.Open();
        }

        public BonoboGitServerContext GetContext()
        {
            return BonoboGitServerContext.FromDatabase(_connection);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}

