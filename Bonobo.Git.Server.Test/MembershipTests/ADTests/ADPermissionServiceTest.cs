using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests.ADTests
{
    [TestClass]
    public class ADPermissionServiceTest : PermissionServiceTestBase
    {
        private ADTestSupport _testSupport;
        private ADRoleProvider _roles;

        [TestInitialize]
        public void Initialize()
        {
            _testSupport = new ADTestSupport();
            InitialiseTestObjects();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _testSupport.Dispose();
        }

        private void InitialiseTestObjects()
        {
            _roles = new ADRoleProvider();
            _teams = new ADTeamRepository();
            _users = new ADMembershipServiceTestFacade(new ADMembershipService(), _testSupport);
            _repos = new ADRepositoryRepository();

            _service = new ADRepositoryPermissionService
            {
                Repository = _repos, RoleProvider = _roles, TeamRepository = _teams
            };
        }

        protected override TeamModel CreateTeam()
        {
            var newTeam = new TeamModel { Name = "Team1" };
            newTeam.Members = new UserModel[0];
            ADBackend.Instance.Teams.Add(newTeam);
            return newTeam;
        }

        protected override void UpdateTeam(TeamModel team)
        {
            ADBackend.Instance.Teams.AddOrUpdate(team);
        }
    }
}