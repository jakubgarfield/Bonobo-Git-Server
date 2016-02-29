using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            _teams = new EFTeamRepository { CreateContext = () => _connection.GetContext() };
            _users = new EFMembershipService { CreateContext = () => _connection.GetContext() };
            _repos = new EFRepositoryRepository { CreateContext = () => _connection.GetContext() };

            _service = new EFRepositoryPermissionService
            {
                CreateContext = () => _connection.GetContext(),
                Repository = _repos,
                TeamRepository = _teams
            };

            new AutomaticUpdater().RunWithContext(_connection.GetContext());
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