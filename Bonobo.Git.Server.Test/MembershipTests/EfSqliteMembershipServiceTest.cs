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
    public class EfSqliteMembershipServiceTest : EFMembershipServiceTest
    {
        SqliteTestConnection _connection;

        [TestInitialize]
        public void Initialize()
        {
            _connection = new SqliteTestConnection();
            _service = new EFMembershipService(MakeContext);
            new AutomaticUpdater().RunWithContext(MakeContext());
        }

        protected override BonoboGitServerContext MakeContext()
        {
            return _connection.GetContext();
        }
    }
}