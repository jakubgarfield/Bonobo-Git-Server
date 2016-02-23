using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests
{
    [TestClass]
    public class EFSqliteTeamRepositoryTests : EFTeamRepositoryTests
    {
        SqliteTestConnection _connection;

        [TestInitialize]
        public void Initialize()
        {
            _connection = new SqliteTestConnection();
            _repo = new EFTeamRepository { CreateContext =  () => _connection.GetContext() };
            new AutomaticUpdater().RunWithContext(_connection.GetContext());
        }

        protected override BonoboGitServerContext GetContext()
        {
            return _connection.GetContext();
        }
    }

    [TestClass]
    public class EFSQlServerTeamRepositoryTests : EFTeamRepositoryTests
    {
        SqlServerTestConnection _connection;

        [TestInitialize]
        public void Initialize()
        {
            _connection = new SqlServerTestConnection();
            _repo = new EFTeamRepository { CreateContext = () => _connection.GetContext() };
            new AutomaticUpdater().RunWithContext(_connection.GetContext());
        }

        protected override BonoboGitServerContext GetContext()
        {
            return _connection.GetContext();
        }
    }

    public abstract class EFTeamRepositoryTests
    {
        protected EFTeamRepository _repo;
        protected abstract BonoboGitServerContext GetContext();

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
            var createResult = _repo.Create(new TeamModel { Name = "Team1", Description = "Test Team" });

            Assert.IsTrue(createResult);
            var addedTeam = _repo.GetAllTeams().Single();
            Assert.AreEqual("Team1", addedTeam.Name);
            Assert.AreEqual("Test Team", addedTeam.Description);
        }

        [TestMethod]
        public void TestNewTeamCanBeRetrievedByIs()
        {
            _repo.Create(new TeamModel { Name = "Team1", Description = "Test Team" });
            var addedTeamId = _repo.GetAllTeams().Single().Id;
            var addedTeam = _repo.GetTeam(addedTeamId);
            Assert.AreEqual("Team1", addedTeam.Name);
        }
        
        [TestMethod]
        public void TestMultipleTeamsCannotHaveSameTeamName()
        {
            var createResult1 = _repo.Create(new TeamModel { Name = "Team1" });
            var createResult2 = _repo.Create(new TeamModel { Name = "Team1" });

            Assert.IsTrue(createResult1);
            Assert.IsFalse(createResult2);
        }

        [TestMethod]
        public void TestMultipleTeamsCanHaveDifferentTeamNames()
        {
            var createResult1 = _repo.Create(new TeamModel { Name = "Team1" });
            var createResult2 = _repo.Create(new TeamModel { Name = "Team2" });

            Assert.IsTrue(createResult1);
            Assert.IsTrue(createResult2);
            Assert.AreEqual(2, _repo.GetAllTeams().Count);
        }

        [TestMethod]
        public void TestNewTeamCanBeAddedWithAMember()
        {
            var newMember = AddUserFred();
            var createResult1 = _repo.Create(new TeamModel {Name = "Team1", Members = new[] {newMember}});
            var createResult = createResult1;

            Assert.IsTrue(createResult);
            var addedTeam = _repo.GetAllTeams().Single();
            Assert.AreEqual("Team1", addedTeam.Name);
            CollectionAssert.AreEqual(new[] { "fred"}, addedTeam.Members.Select(user => user.Username).ToArray());
        }

        [TestMethod]
        public void DuplicateMemberIsSilentlyIgnored()
        {
            var newMember = AddUserFred();
            var createResult = _repo.Create(new TeamModel { Name = "Team1", Members = new[] { newMember, newMember } });

            Assert.IsTrue(createResult);
            Assert.AreEqual(1, _repo.GetAllTeams().Single().Members.Length);
        }

        [TestMethod]
        public void NewUserIsNotATeamMember()
        {
            var newMember = AddUserFred();
            Assert.AreEqual(0, _repo.GetTeams(newMember.Id).Count);
        }

        [TestMethod]
        public void TeamCanBeUpdatedToIncludeAUser()
        {
            var team1 = new TeamModel { Name = "Team1", Description = "Test Team" };
            _repo.Create(team1);

            var newUser = AddUserFred();

            _repo.UpdateUserTeams(newUser.Id, new List<string> { "Team1"});

            Assert.AreEqual(1, _repo.GetTeams(newUser.Id).Count);
            CollectionAssert.AreEqual(new[] { newUser.Id }, _repo.GetTeam(team1.Id).Members.Select(member => member.Id).ToArray());
        }

        [TestMethod]
        public void TeamCanBeUpdatedToChangeName()
        {
            var teamModel = new TeamModel { Name = "Team1", Description = "Test Team" };
            _repo.Create(teamModel);

            teamModel.Name = "SonOfTeam1";
            _repo.Update(teamModel);

            Assert.AreEqual("SonOfTeam1", _repo.GetAllTeams().Single().Name);
        }

        [TestMethod]
        public void TeamCanBeDeleted()
        {
            var team1 = new TeamModel { Name = "Team1", Description = "Test Team" };
            _repo.Create(team1);
            var team2 = new TeamModel { Name = "Team2", Description = "Test Team" };
            _repo.Create(team2);

            _repo.Delete(team1.Id);

            Assert.AreEqual("Team2", _repo.GetAllTeams().Single().Name);
        }

        [TestMethod]
        public void DeletingMissingTeamIsSilentlyIgnored()
        {
            var team1 = new TeamModel { Name = "Team1", Description = "Test Team" };
            _repo.Create(team1);

            _repo.Delete(Guid.NewGuid());

            Assert.AreEqual("Team1", _repo.GetAllTeams().Single().Name);
        }


        private UserModel AddUserFred()
        {
            EFMembershipService memberService = new EFMembershipService { CreateContext = GetContext };
            memberService.CreateUser("fred", "letmein", "Fred", "FredBlogs", "fred@aol");
            return memberService.GetUserModel("fred");
        }

    }
}