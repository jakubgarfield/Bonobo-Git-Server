using System;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests
{
    [TestClass]
    class ADRepositoryRepositoryServiceTest : RepositoryRepositoryTestBase
    {
        private ADTestSupport _testSupport;

        [TestInitialize]
        public void Initialize()
        {
            _testSupport = new ADTestSupport();
            _repo = new ADRepositoryRepository();
/*
            _service = new ADMembershipServiceTestFacade(new ADMembershipService());
            ADBackend.Instance.Users.Add(new UserModel() { Username = "admin", Id = Guid.NewGuid() });
            Assert.AreEqual(1, ADBackend.Instance.Users.Count());
*/
        }

        [TestCleanup]
        public void Cleanup()
        {
            _testSupport.Dispose();
        }

        protected override UserModel AddUserFred()
        {
            return _testSupport.CreateUser("fred", "letmein", "Fred", "Blogs", "fred@aol", Guid.NewGuid());
        }

        protected override TeamModel AddTeam()
        {
            var newTeam = new TeamModel { Name = "Team1"};
            ADBackend.Instance.Teams.Add(newTeam);
            return newTeam;
        }
    }
}