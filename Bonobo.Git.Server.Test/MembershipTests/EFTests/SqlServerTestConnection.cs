using System;
using System.Data.SqlClient;
using System.IO;
using Bonobo.Git.Server.Data;

namespace Bonobo.Git.Server.Test.MembershipTests.EFTests
{
    class SqlServerTestConnection : IDatabaseTestConnection
    {
        readonly SqlConnection _connection;
        private readonly string _databaseName;
        private static readonly string _instanceName;

        static SqlServerTestConnection()
        {
            // If you need to find the instance names on your computer run "sqllocaldb info" at the command prompt 
            var instances = new[] {"v11.0", "MSSQLLocalDB"};
            foreach (var instanceName in instances)
            {
                if (TryOpeningInstance(instanceName))
                {
                    _instanceName = instanceName;
                    break;
                }
            }
        }
    
        private static bool TryOpeningInstance(string instanceName)
        {
            try
            {
                using (var conn = 
                    new SqlConnection(
                        string.Format(@"Data Source=(LocalDb)\{0};Initial Catalog=Master;Integrated Security=True",
                            instanceName)))
                {
                    conn.Open();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public SqlServerTestConnection()
        {
            _databaseName = Guid.NewGuid().ToString();
            var fileName = Path.Combine(Path.GetTempPath(), "BonoboTestDb_" + _databaseName + ".mdf");
            CreateDB(fileName);

            Console.WriteLine("Created test database: " + fileName);

            _connection = new SqlConnection(String.Format(@"Data Source=(LocalDB)\{0};Integrated Security=True;AttachDbFilename={1};Initial Catalog={2}", _instanceName, fileName, _databaseName));
            _connection.Open();
        }

        public BonoboGitServerContext GetContext()
        {
            return BonoboGitServerContext.FromDatabase(_connection);
        }

        void CreateDB(string fileName)
        {
            using (
                var connection =
                    new SqlConnection(string.Format(@"Data Source=(LocalDb)\{0};Initial Catalog=Master;Integrated Security=True", _instanceName)))
            {
                connection.Open();

/*                var cmd = connection.CreateCommand();
                cmd.CommandText = string.Format(@"
	                IF EXISTS(SELECT * FROM sys.databases WHERE name='{0}')
	                BEGIN
		                ALTER DATABASE [{0}]
		                SET SINGLE_USER
		                WITH ROLLBACK IMMEDIATE
		                DROP DATABASE [{0}]
	                END",
                    _databaseName);
                cmd.ExecuteNonQuery();*/

                var cmd2 = connection.CreateCommand();
                cmd2.CommandText = string.Format(@"

                    DECLARE @FILENAME AS VARCHAR(255)
                    SET @FILENAME = CONVERT(VARCHAR(255), '{1}');

	                EXEC ('CREATE DATABASE [{0}] ON PRIMARY 
		                (NAME = [{0}], 
		                FILENAME = ''' +@FILENAME + ''', 
		                SIZE = 5MB, 
		                MAXSIZE = 10MB, 
		                FILEGROWTH = 5MB )')",
                    _databaseName, fileName);
                cmd2.ExecuteNonQuery();

                Exec(connection, string.Format(@"ALTER DATABASE [{0}] SET AUTO_CLOSE ON;", _databaseName));
//                Exec(connection, string.Format(@"ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", _databaseName));
            }
        }

        private void Exec(SqlConnection connection, string commandText)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.ExecuteNonQuery();
        }

        public void Dispose()
        {
            _connection.Dispose();
            SqlConnection.ClearAllPools();
            TryToDeleteDatabaseFiles();
        }

        private static void TryToDeleteDatabaseFiles()
        {
            foreach (var dbFile in Directory.EnumerateFiles(Path.GetTempPath(), "BonoboTestDb_*"))
            {
                try
                {
                    File.Delete(dbFile);
                }
                catch
                {
                    // Don't worry if we can't delete the files
                }
            }
        }
    }
}

