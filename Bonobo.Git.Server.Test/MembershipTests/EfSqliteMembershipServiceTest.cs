using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests
{
    /// <summary>
    /// EF Membership tests using in-memory Sqlite
    /// </summary>
    [TestClass]
    public class EFSqliteMembershipServiceTest : EFMembershipServiceTest
    {
        SqliteTestConnection _connection;

        [TestInitialize]
        public void Initialize()
        {
            _connection = new SqliteTestConnection();
            _service = new EFMembershipService { CreateContext = GetContext };
            new AutomaticUpdater().RunWithContext(GetContext());
        }

        protected override BonoboGitServerContext GetContext()
        {
            return _connection.GetContext();
        }
    }
}