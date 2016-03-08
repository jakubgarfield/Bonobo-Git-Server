using System.Linq;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests
{
    public abstract class TeamRepositoryTestsBase
    {
        protected ITeamRepository _repo;
        protected IMembershipService _membershipService;

        [TestMethod]
        public void TestRepositoryIsCreated()
        {
            Assert.IsNotNull(_repo);
        }

        [TestMethod]
        public void TestNewRepositoryIsEmpty()
        {
            Assert.AreEqual(0, _repo.GetAllTeams().Count);
        }

        [TestMethod]
        public void TestNewTeamCanBeAddedWithNoMembers()
        {
            var createResult = CreateTeam(new TeamModel { Name = "Team1", Description = "Test Team" });

            Assert.IsTrue(createResult);
            var addedTeam = _repo.GetAllTeams().Single();
            Assert.AreEqual("Team1", addedTeam.Name);
            Assert.AreEqual("Test Team", addedTeam.Description);
        }

        [TestMethod]
        public void TestNewTeamCanBeRetrievedByIs()
        {
            CreateTeam(new TeamModel { Name = "Team1", Description = "Test Team" });
            var addedTeamId = _repo.GetAllTeams().Single().Id;
            var addedTeam = _repo.GetTeam(addedTeamId);
            Assert.AreEqual("Team1", addedTeam.Name);
        }

        [TestMethod]
        public void TestGetTeamByNameIsCaseInsensitive()
        {
            var createResult = CreateTeam(new TeamModel { Name = "Team1" });
            Assert.IsTrue(createResult);
            Assert.AreEqual("Team1", _repo.GetTeam("Team1").Name);
            Assert.AreEqual("Team1", _repo.GetTeam("team1").Name);
            Assert.AreEqual("Team1", _repo.GetTeam("TEAM1").Name);
        }

        [TestMethod]
        public void TestMultipleTeamsCanHaveDifferentTeamNames()
        {
            var createResult1 = CreateTeam(new TeamModel { Name = "Team1" });
            var createResult2 = CreateTeam(new TeamModel { Name = "Team2" });

            Assert.IsTrue(createResult1);
            Assert.IsTrue(createResult2);
            Assert.AreEqual(2, _repo.GetAllTeams().Count);
        }

        [TestMethod]
        public void TestNewTeamCanBeAddedWithAMember()
        {
            var newMember = AddUserFred();
            var createResult1 = CreateTeam(new TeamModel {Name = "Team1", Members = new[] {newMember}});
            var createResult = createResult1;

            Assert.IsTrue(createResult);
            var addedTeam = _repo.GetAllTeams().Single();
            Assert.AreEqual("Team1", addedTeam.Name);
            CollectionAssert.AreEqual(new[] { "fred"}, addedTeam.Members.Select(user => user.Username).ToArray());
        }

        [TestMethod]
        public void NewUserIsNotATeamMember()
        {
            var newMember = AddUserFred();
            Assert.AreEqual(0, _repo.GetTeams(newMember.Id).Count);
        }

        protected UserModel AddUserFred()
        {
            _membershipService.CreateUser("fred", "letmein", "Fred", "FredBlogs", "fred@aol");
            return _membershipService.GetUserModel("fred");
        }

        protected abstract bool CreateTeam(TeamModel team);
    }
}