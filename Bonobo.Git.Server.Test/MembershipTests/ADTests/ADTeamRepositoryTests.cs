using System;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests.ADTests
{
    [TestClass]
    public class ADTeamRepositoryTests : TeamRepositoryTestsBase
    {
        private ADTestSupport _testSupport;

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

        void InitialiseTestObjects()
        {
            _repo = new ADTeamRepository();
            _membershipService = new ADMembershipServiceTestFacade(new ADMembershipService(), _testSupport);
        }

        protected override bool CreateTeam(TeamModel team)
        {
            team.Id = Guid.NewGuid();
            ADBackend.Instance.Teams.Add(team);
            return true;
        }
    }
}