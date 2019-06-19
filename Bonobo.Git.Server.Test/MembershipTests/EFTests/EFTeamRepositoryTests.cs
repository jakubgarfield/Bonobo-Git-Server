using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Bonobo.Git.Server.Test.MembershipTests.EFTests
{
    [TestClass]
    public class EFSqliteTeamRepositoryTests : EFTeamRepositoryTests
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
    public class EFSqlServerTeamRepositoryTests : EFTeamRepositoryTests
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

    public abstract class EFTeamRepositoryTests : TeamRepositoryTestsBase
    {
        protected IDatabaseTestConnection _connection;

        protected void InitialiseTestObjects()
        {
            _repo = new EFTeamRepository(() => _connection.GetContext());
            _membershipService = new EFMembershipService(() => _connection.GetContext());
            new AutomaticUpdater().RunWithContext(_connection.GetContext(), Substitute.For<IAuthenticationProvider>());
        }

        protected override bool CreateTeam(TeamModel team)
        {
            return _repo.Create(team);
        }

        [TestMethod]
        public void DeletingMissingTeamIsSilentlyIgnored()
        {
            var team1 = new TeamModel { Name = "Team1", Description = "Test Team" };
            CreateTeam(team1);

            _repo.Delete(Guid.NewGuid());

            Assert.AreEqual("Team1", _repo.GetAllTeams().Single().Name);
        }

        [TestMethod]
        public void TeamCanBeDeleted()
        {
            var team1 = new TeamModel { Name = "Team1", Description = "Test Team" };
            CreateTeam(team1);
            var team2 = new TeamModel { Name = "Team2", Description = "Test Team" };
            CreateTeam(team2);

            _repo.Delete(team1.Id);

            Assert.AreEqual("Team2", _repo.GetAllTeams().Single().Name);
        }

        [TestMethod]
        public void TeamCanBeUpdatedToIncludeAUser()
        {
            var team1 = new TeamModel { Name = "Team1", Description = "Test Team" };
            CreateTeam(team1);

            var newUser = AddUserFred();

            _repo.UpdateUserTeams(newUser.Id, new List<string> { "Team1"});

            Assert.AreEqual(1, _repo.GetTeams(newUser.Id).Count);
            CollectionAssert.AreEqual(new[] { newUser.Id }, _repo.GetTeam(team1.Id).Members.Select(member => member.Id).ToArray());
        }

        [TestMethod]
        public void TeamCanBeUpdatedToChangeName()
        {
            var teamModel = new TeamModel { Name = "Team1", Description = "Test Team" };
            CreateTeam(teamModel);

            teamModel.Name = "SonOfTeam1";
            _repo.Update(teamModel);

            Assert.AreEqual("SonOfTeam1", _repo.GetAllTeams().Single().Name);
        }

        [TestMethod]
        public void TestMultipleTeamsCannotHaveSameTeamName()
        {
            var createResult1 = CreateTeam(new TeamModel { Name = "Team1" });
            var createResult2 = CreateTeam(new TeamModel { Name = "Team1" });

            Assert.IsTrue(createResult1);
            Assert.IsFalse(createResult2);
        }

        [TestMethod]
        public void DuplicateMemberIsSilentlyIgnored()
        {
            var newMember = AddUserFred();
            var createResult = CreateTeam(new TeamModel { Name = "Team1", Members = new[] { newMember, newMember } });

            Assert.IsTrue(createResult);
            Assert.AreEqual(1, _repo.GetAllTeams().Single().Members.Length);
        }
    }
}