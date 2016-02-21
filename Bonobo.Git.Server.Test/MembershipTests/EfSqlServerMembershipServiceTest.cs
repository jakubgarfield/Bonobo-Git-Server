using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests
{
    /// <summary>
    /// EF Membership tests using SQL Server
    /// </summary>
    [TestClass]
    public class EfSqlServerMembershipServiceTest : EFMembershipServiceTest
    {
        SqlServerTestConnection _connection;

        [TestInitialize]
        public void Initialize()
        {
            _connection = new SqlServerTestConnection();
            new AutomaticUpdater().RunWithContext(_connection.GetContext());
            _service = new EFMembershipService(() => _connection.GetContext());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _connection.Dispose();
        }

        protected override BonoboGitServerContext MakeContext()
        {
            return _connection.GetContext();
        }
    }
}