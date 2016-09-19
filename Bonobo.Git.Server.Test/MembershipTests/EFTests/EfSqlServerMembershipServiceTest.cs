using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests.EFTests
{
    /// <summary>
    /// EF Membership tests using SQL Server
    /// </summary>
    [TestClass]
    public class EFSqlServerMembershipServiceTest : EFMembershipServiceTest
    {
        [TestInitialize]
        public void Initialize()
        {
            _connection = new SqlServerTestConnection();
            InitialiseTestObjects();
        }
        [TestCleanup]
        public void Cleanup()
        {
            _connection.Dispose();
        }
    }
}