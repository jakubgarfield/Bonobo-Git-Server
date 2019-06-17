using System;
using System.Data.Common;
using Bonobo.Git.Server.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bonobo.Git.Server.Test.MembershipTests.EFTests
{
    public interface IDatabaseTestConnection : IDisposable
    {
        BonoboGitServerContext GetContext();
    }

    class SqliteTestConnection : IDatabaseTestConnection
    {
        private readonly DbConnection _connection;

        public SqliteTestConnection()
        {
            _connection = SqliteFactory.Instance.CreateConnection();
            _connection.ConnectionString = "Data Source =:memory:";
            _connection.Open();
        }

        public BonoboGitServerContext GetContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<BonoboGitServerContext>();
            optionsBuilder.UseSqlite(_connection);
            return new BonoboGitServerContext(optionsBuilder.Options);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}

