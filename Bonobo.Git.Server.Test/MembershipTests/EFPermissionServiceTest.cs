using System;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests
{
    [TestClass]
    public class EFSqlitePermissionServiceTest : EFPermissionServiceTest
    {
        private SqliteTestConnection _connection;

        [TestInitialize]
        public void Initialize()
        {
            _connection = new SqliteTestConnection();
            _service = new EFRepositoryPermissionService
            {
                CreateContext = () => _connection.GetContext(),
                Repository = new EFRepositoryRepository { CreateContext = () => _connection.GetContext() }
            };
            new AutomaticUpdater().RunWithContext(_connection.GetContext());
        }

        protected override BonoboGitServerContext GetContext()
        {
            return _connection.GetContext();
        }
    }

    [TestClass]
    public class EfSqlServerPermissionServiceTest : EFPermissionServiceTest
    {
        private SqlServerTestConnection _connection;

        [TestInitialize]
        public void Initialize()
        {
            _connection = new SqlServerTestConnection();
            _service = new EFRepositoryPermissionService
            {
                CreateContext = () => _connection.GetContext(),
                Repository = new EFRepositoryRepository() { CreateContext = () => _connection.GetContext() }
            };
            new AutomaticUpdater().RunWithContext(_connection.GetContext());
        }

        protected override BonoboGitServerContext GetContext()
        {
            return _connection.GetContext();
        }
    }

    public abstract class EFPermissionServiceTest
    {
        protected IRepositoryPermissionService _service;
        protected abstract BonoboGitServerContext GetContext();

        [TestMethod]
        public void NonExistentRepositoryByNameReturnsFalse()
        {
            var adminId = GetAdminId();
            Assert.IsFalse(_service.HasPermission(adminId, "NonExistentRepos"));
        }

        [TestMethod]
        public void NonExistentRepositoryByGuidReturnsFalse()
        {
            var adminId = GetAdminId();
            Assert.IsFalse(_service.HasPermission(adminId, Guid.NewGuid()));
        }

        [TestMethod]
        public void AdminIsAuthorisedForAnyRepo()
        {
            var adminId = GetAdminId();
            var repoId = AddRepo("TestRepo");
            Assert.IsTrue(CheckPermission(adminId, repoId));
        }

        [TestMethod]
        public void UnrelatedUserIsNotAuthorisedForRepo()
        {
            var user = AddUser();
            var repoId = AddRepo("TestRepo");
            Assert.IsFalse(CheckPermission(user.Id, repoId));
        }

        [TestMethod]
        public void RepoMemberUserIsAuthorised()
        {
            var user = AddUser();
            var repoId = AddRepo("TestRepo");
            AddUserToRepo(repoId, user);

            Assert.IsTrue(CheckPermission(user.Id, repoId));
        }

        [TestMethod]
        public void RepoAdminIsAuthorised()
        {
            var user = AddUser();
            var repoId = AddRepo("TestRepo");
            AddAdminToRepo(repoId, user);

            Assert.IsTrue(CheckPermission(user.Id, repoId));
        }

        [TestMethod]
        public void NonTeamMemberIsNotAuthorised()
        {
            var user = AddUser();
            var repoId = AddRepo("TestRepo");
            var team = CreateTeam();
            AddTeamToRepo(repoId,team);
            Assert.IsFalse(CheckPermission(user.Id, repoId));
        }

        [TestMethod]
        public void TeamMemberIsAuthorised()
        {
            var user = AddUser();
            var repoId = AddRepo("TestRepo");
            var team = CreateTeam();
            AddTeamToRepo(repoId, team);

            // Add the member to the team
            team.Members = new[] {user};
            EFTeamRepository teams = new EFTeamRepository { CreateContext = GetContext };
            teams.Update(team);

            Assert.IsTrue(CheckPermission(user.Id, repoId));
        }

        [TestMethod]
        public void SystemAdminIsAlwaysRepositoryAdmin()
        {
            var repoId = AddRepo("TestRepo");
            Assert.IsTrue(_service.IsRepositoryAdministrator(GetAdminId(), repoId));
        }

        [TestMethod]
        public void NormalUserIsNotRepositoryAdmin()
        {
            var user = AddUser();
            var repoId = AddRepo("TestRepo");
            AddUserToRepo(repoId, user);
            Assert.IsFalse(_service.IsRepositoryAdministrator(user.Id, repoId));
        }

        [TestMethod]
        public void AdminUserIsRepositoryAdmin()
        {
            var user = AddUser();
            var repoId = AddRepo("TestRepo");
            AddAdminToRepo(repoId, user);
            Assert.IsTrue(_service.IsRepositoryAdministrator(user.Id, repoId));
        }

        [TestMethod]
        public void DefaultRepositoryDoesNotAllowAnonAccess()
        {
            var repoId = AddRepo("TestRepo");
            Assert.IsFalse(_service.AllowsAnonymous(repoId));
            Assert.IsFalse(_service.AllowsAnonymous("TestRepo"));
        }

        [TestMethod]
        public void UnknownRepositoryDoesNotAllowAnonAccess()
        {
            Assert.IsFalse(_service.AllowsAnonymous(Guid.NewGuid()));
            Assert.IsFalse(_service.AllowsAnonymous("UnknownRepo"));
        }

        [TestMethod]
        public void AnonAccessCanBePermitted()
        {
            var repoId = AddRepo("TestRepo");
            UpdateRepo(repoId, repo => repo.AnonymousAccess = true);
            Assert.IsTrue(_service.AllowsAnonymous(repoId));
            Assert.IsTrue(_service.AllowsAnonymous("TestRepo"));
        }

        /// <summary>
        /// A check-permission routine which runs checks by both name and Guid, and makes sure they agree
        /// </summary>
        private bool CheckPermission(Guid userId, Guid repoId)
        {
            bool byGuid = _service.HasPermission(userId, repoId);
            EFRepositoryRepository repoRepo = new EFRepositoryRepository { CreateContext = GetContext };
            bool byName = _service.HasPermission(userId, repoRepo.GetRepository(repoId).Name);
            Assert.IsTrue(byGuid == byName);
            return byGuid;
        }

        private Guid AddRepo(string name)
        {
            var newRepo = new RepositoryModel();
            newRepo.Name = name;
            newRepo.Users = new UserModel[0];
            newRepo.Administrators = new UserModel[0];
            newRepo.Teams = new TeamModel[0];

            EFRepositoryRepository repoRepo = new EFRepositoryRepository { CreateContext = GetContext };
            Assert.IsTrue(repoRepo.Create(newRepo));
            return newRepo.Id;
        }

        private void AddUserToRepo(Guid repoId, UserModel user)
        {
            UpdateRepo(repoId, repo => repo.Users = new[] { user });
        }

        private void AddAdminToRepo(Guid repoId, UserModel adminUser)
        {
            UpdateRepo(repoId, repo => repo.Administrators = new[] { adminUser });
        }

        private void AddTeamToRepo(Guid repoId, TeamModel team)
        {
            UpdateRepo(repoId, repo => repo.Teams = new[] { team });
        }

        private void UpdateRepo(Guid repoId, Action<RepositoryModel> transform)
        {
            EFRepositoryRepository repoRepo = new EFRepositoryRepository { CreateContext = GetContext };
            var repo = repoRepo.GetRepository(repoId);
            transform(repo);
            repoRepo.Update(repo);
        }

        private UserModel AddUser()
        {
            EFMembershipService memberService = new EFMembershipService { CreateContext = GetContext };
            memberService.CreateUser("fred", "letmein", "Fred", "FredBlogs", "fred@aol");
            return memberService.GetUserModel("fred");
        }

        private TeamModel CreateTeam()
        {
            EFTeamRepository teams = new EFTeamRepository { CreateContext = GetContext };
            var newTeam = new TeamModel { Name = "Team1" };
            teams.Create(newTeam);
            return newTeam;
        }

        private Guid GetAdminId()
        {
            EFMembershipService memberService = new EFMembershipService { CreateContext = GetContext };
            return memberService.GetUserModel("Admin").Id;
        }
    }
}