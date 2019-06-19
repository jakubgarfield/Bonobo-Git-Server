using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Bonobo.Git.Server.Test.MembershipTests.EFTests
{
    [TestClass]
    public class EFSqlitePermissionServiceTest : EFPermissionServiceTest
    {
        [TestInitialize]
        public void Initialize()
        {
            _connection = new SqliteTestConnection();
            InitialiseTestObjects();
        }
        [TestCleanup]
        public void Cleanup()
        {
            _connection.Dispose();
        }
    }

    [TestClass]
    public class EfSqlServerPermissionServiceTest : EFPermissionServiceTest
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

    public abstract class EFPermissionServiceTest : PermissionServiceTestBase
    {
        protected IDatabaseTestConnection _connection;

        protected void InitialiseTestObjects()
        {
            _teams = new EFTeamRepository(() => _connection.GetContext());
            _users = new EFMembershipService(() => _connection.GetContext());
            _repos = new EFRepositoryRepository(() => _connection.GetContext());
            _roles = new EFRoleProvider(() => _connection.GetContext() );

            _service = new RepositoryPermissionService(_repos, _roles, _teams);

            new AutomaticUpdater().RunWithContext(_connection.GetContext(), Substitute.For<IAuthenticationProvider>());
        }

        protected override TeamModel CreateTeam()
        {
            var newTeam = new TeamModel { Name = "Team1" };
            _teams.Create(newTeam);
            return newTeam;
        }

        protected override void UpdateTeam(TeamModel team)
        {
            _teams.Update(team);
        }
    }
}