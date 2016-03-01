using System;
using System.Linq;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests
{
    public abstract class PermissionServiceTestBase
    {
        protected IRepositoryPermissionService _service;
        protected ITeamRepository _teams;
        protected IMembershipService _users;
        protected IRepositoryRepository _repos;
        protected IRoleProvider _roles;

        [TestMethod]
        public void NonExistentRepositoryByNameReturnsFalse()
        {
            var adminId = GetAdminId();
            Assert.IsFalse(_service.HasPermission(adminId, "NonExistentRepos"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void NonExistentRepositoryByGuidThrowsException()
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
            UpdateTeam(team);

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
        public void UnknownRepositoryByNameDoesNotAllowAnonAccess()
        {
            Assert.IsFalse(_service.AllowsAnonymous("UnknownRepo"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UnknownRepositoryByGuidThrowsException()
        {
            Assert.IsFalse(_service.AllowsAnonymous(Guid.NewGuid()));
        }

        [TestMethod]
        public void AnonAccessCanBePermitted()
        {
            var repoId = AddRepo("TestRepo");
            UpdateRepo(repoId, repo => repo.AnonymousAccess = true);
            Assert.IsTrue(_service.AllowsAnonymous(repoId));
            Assert.IsTrue(_service.AllowsAnonymous("TestRepo"));
        }

        [TestMethod]
        public void GetAllPermittedReturnsOnlyRepositoriesPermittedForUser()
        {
            var user = AddUser();
            var repo1 = AddRepo("TestRepo1");
            var repo2 = AddRepo("TestRepo2");
            var repo3 = AddRepo("TestRepo3");
            AddUserToRepo(repo1, user);
            AddUserToRepo(repo3, user);

            CollectionAssert.AreEqual(new[] { "TestRepo1", "TestRepo3" },
                _service.GetAllPermittedRepositories(user.Id).Select(r => r.Name).OrderBy(r => r).ToArray());
        }

        [TestMethod]
        public void GetAllPermittedReturnsAllRepositoriesToSystemAdmin()
        {
            AddRepo("TestRepo1");
            AddRepo("TestRepo2");
            AddRepo("TestRepo3");

            CollectionAssert.AreEqual(new[] { "TestRepo1", "TestRepo2", "TestRepo3" },
                _service.GetAllPermittedRepositories(GetAdminId()).Select(r => r.Name).OrderBy(r => r).ToArray());
        }

        [TestMethod]
        public void AnonymousRepoIsPermittedToAnybody()
        {
            var repo = MakeRepo("Repo1");
            repo.AnonymousAccess = true;
            Assert.IsTrue(_repos.Create(repo));

            var anonymousUser = Guid.Empty;
            Assert.AreEqual("Repo1", _service.GetAllPermittedRepositories(anonymousUser).Single().Name);
        }

        [TestMethod]
        public void RepositoryIsPermittedToUser()
        {
            var user = AddUser();
            var repoWithUser = MakeRepo("Repo1");
            repoWithUser.Users = new[] { user };
            Assert.IsTrue(_repos.Create(repoWithUser));
            AddRepo("Repo2");
            
            Assert.AreEqual("Repo1", _service.GetAllPermittedRepositories(user.Id).Single().Name);
        }

        [TestMethod]
        public void NewRepositoryNotPermittedToUnknownUser()
        {
            var user = AddUser();
            var repoWithUser = MakeRepo("Repo1");
            repoWithUser.Users = new[] { user };
            Assert.IsTrue(_repos.Create(repoWithUser));

            var unknownUserId = Guid.Empty;
            Assert.IsFalse(_service.GetAllPermittedRepositories(unknownUserId).Any());
        }

        [TestMethod]
        public void RepositoryIsPermittedToRepoAdministrator()
        {
            var user = AddUser();
            var repoWithAdmin = MakeRepo("Repo1");
            repoWithAdmin.Administrators = new[] { user };
            Assert.IsTrue(_repos.Create(repoWithAdmin));
            AddRepo("Repo2");

            Assert.AreEqual("Repo1", _service.GetAllPermittedRepositories(user.Id).Single().Name);
        }

        /// <summary>
        /// A check-permission routine which runs checks by both name and Guid, and makes sure they agree
        /// </summary>
        private bool CheckPermission(Guid userId, Guid repoId)
        {
            bool byGuid = _service.HasPermission(userId, repoId);
            bool byName = _service.HasPermission(userId, _repos.GetRepository(repoId).Name);
            Assert.IsTrue(byGuid == byName);
            return byGuid;
        }

        private UserModel AddUser()
        {
            _users.CreateUser("fred", "letmein", "Fred", "FredBlogs", "fred@aol");
            return _users.GetUserModel("fred");
        }

        protected abstract TeamModel CreateTeam();

        private Guid GetAdminId()
        {
            return _users.GetUserModel("Admin").Id;
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

        protected abstract void UpdateTeam(TeamModel team);

        void UpdateRepo(Guid repoId, Action<RepositoryModel> transform)
        {
            var repo = _repos.GetRepository(repoId);
            transform(repo);
            _repos.Update(repo);
        }

        Guid AddRepo(string name)
        {
            var newRepo = MakeRepo(name);
            Assert.IsTrue(_repos.Create(newRepo));
            return newRepo.Id;
        }

        RepositoryModel MakeRepo(string name)
        {
            var newRepo = new RepositoryModel();
            newRepo.Name = name;
            return newRepo;
        }
    }
}