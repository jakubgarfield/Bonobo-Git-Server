using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Bonobo.Git.Server.Configuration;
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

        [TestInitialize]
        public void Initialse()
        {
            // This file should never actually get created, but ConfigurationManager needs it for its static initialisation
            var configFileName = Path.Combine(Path.GetTempFileName(), "BonoboTestConfig.xml");
            ConfigurationManager.AppSettings["UserConfiguration"] = configFileName;
            UserConfiguration.InitialiseForTest();
        }

        [TestMethod]
        public void NonExistentRepositoryByNameReturnsFalse()
        {
            var adminId = GetAdminId();
            Assert.IsFalse(_service.HasPermission(adminId, "NonExistentRepos", RepositoryAccessLevel.Pull));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void NonExistentRepositoryByGuidThrowsException()
        {
            var adminId = GetAdminId();
            Assert.IsFalse(_service.HasPermission(adminId, Guid.NewGuid(), RepositoryAccessLevel.Pull));
        }

        [TestMethod]
        public void AdminIsAuthorisedForAnyRepo()
        {
            var adminId = GetAdminId();
            var repoId = AddRepo("TestRepo");
            Assert.IsTrue(CheckPermission(adminId, repoId, RepositoryAccessLevel.Pull));
        }

        [TestMethod]
        public void UnrelatedUserIsNotAuthorisedForRepo()
        {
            var user = AddUser();
            var repoId = AddRepo("TestRepo");
            Assert.IsFalse(CheckPermission(user.Id, repoId, RepositoryAccessLevel.Pull));
        }

        [TestMethod]
        public void RepoMemberUserIsAuthorised()
        {
            var user = AddUser();
            var repoId = AddRepo("TestRepo");
            AddUserToRepo(repoId, user);

            Assert.IsTrue(CheckPermission(user.Id, repoId, RepositoryAccessLevel.Pull));
        }

        [TestMethod]
        public void RepoAdminIsAuthorised()
        {
            var user = AddUser();
            var repoId = AddRepo("TestRepo");
            AddAdminToRepo(repoId, user);

            Assert.IsTrue(CheckPermission(user.Id, repoId, RepositoryAccessLevel.Pull));
        }

        [TestMethod]
        public void NonTeamMemberIsNotAuthorised()
        {
            var user = AddUser();
            var repoId = AddRepo("TestRepo");
            var team = CreateTeam();
            AddTeamToRepo(repoId,team);
            Assert.IsFalse(CheckPermission(user.Id, repoId, RepositoryAccessLevel.Pull));
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

            Assert.IsTrue(CheckPermission(user.Id, repoId, RepositoryAccessLevel.Pull));
        }

        [TestMethod]
        public void SystemAdminIsAlwaysRepositoryAdmin()
        {
            var repoId = AddRepo("TestRepo");
            Assert.IsTrue(_service.HasPermission(GetAdminId(), repoId, RepositoryAccessLevel.Administer));
        }

        [TestMethod]
        public void NormalUserIsNotRepositoryAdmin()
        {
            var user = AddUser();
            var repoId = AddRepo("TestRepo");
            AddUserToRepo(repoId, user);
            Assert.IsFalse(_service.HasPermission(user.Id, repoId, RepositoryAccessLevel.Administer));
        }

        [TestMethod]
        public void AdminUserIsRepositoryAdmin()
        {
            var user = AddUser();
            var repoId = AddRepo("TestRepo");
            AddAdminToRepo(repoId, user);
            Assert.IsTrue(_service.HasPermission(user.Id, repoId, RepositoryAccessLevel.Administer));
        }

        [TestMethod]
        public void DefaultRepositoryDoesNotAllowAnonAccess()
        {
            var repoId = AddRepo("TestRepo");
            Assert.IsFalse(_service.HasPermission(Guid.Empty, repoId, RepositoryAccessLevel.Pull));
            Assert.IsFalse(_service.HasPermission(Guid.Empty, "TestRepo", RepositoryAccessLevel.Pull));
        }

        [TestMethod]
        public void AllowAnonymousPushDoesNotAffectDefaultRepository()
        {
            var repoId = AddRepo("TestRepo");
            // Allow anon push gobally - it shouldn't have any effect because the repo is not enabled for anon access
            UserConfiguration.Current.AllowAnonymousPush = true;
            Assert.IsFalse(_service.HasPermission(Guid.Empty, repoId, RepositoryAccessLevel.Pull));
        }

        [TestMethod]
        public void UnknownRepositoryByNameDoesNotAllowAnonAccess()
        {
            Assert.IsFalse(_service.HasPermission(Guid.Empty, "Unknown", RepositoryAccessLevel.Pull));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UnknownRepositoryByGuidThrowsException()
        {
            Assert.IsFalse(_service.HasPermission(Guid.Empty, Guid.NewGuid(), RepositoryAccessLevel.Pull));
        }

        [TestMethod]
        public void AnonAccessCanBePermittedWithRepoProperty()
        {
            var repoId = AddRepo("TestRepo");
            UpdateRepo(repoId, repo => repo.AnonymousAccess = true);
            Assert.IsTrue(_service.HasPermission(Guid.Empty, repoId, RepositoryAccessLevel.Pull));
            Assert.IsTrue(_service.HasPermission(Guid.Empty, "TestRepo", RepositoryAccessLevel.Pull));
        }

        [TestMethod]
        public void AnonAccessDoesNotAllowPushByDefault()
        {
            var repoId = AddRepo("TestRepo");
            UpdateRepo(repoId, repo => repo.AnonymousAccess = true);
            Assert.IsFalse(_service.HasPermission(Guid.Empty, repoId, RepositoryAccessLevel.Push));
            Assert.IsFalse(_service.HasPermission(Guid.Empty, "TestRepo", RepositoryAccessLevel.Push));
        }

        [TestMethod]
        public void AnonPushCanBeEnabledWithConfig()
        {
            var repoId = AddRepo("TestRepo");
            UpdateRepo(repoId, repo => repo.AnonymousAccess = true);
            UserConfiguration.Current.AllowAnonymousPush = true;
            Assert.IsTrue(_service.HasPermission(Guid.Empty, repoId, RepositoryAccessLevel.Push));
        }

        [TestMethod]
        public void GetAllPermittedReturnsOnlyRepositoriesPermittedForUser()
        {
            var user = AddUser();
            var repo1 = AddRepo("TestRepo1");
            AddRepo("TestRepo2");
            var repo3 = AddRepo("TestRepo3");
            AddUserToRepo(repo1, user);
            AddUserToRepo(repo3, user);

            CollectionAssert.AreEqual(new[] { "TestRepo1", "TestRepo3" },
                _service.GetAllPermittedRepositories(user.Id, RepositoryAccessLevel.Pull).Select(r => r.Name).OrderBy(r => r).ToArray());
        }

        [TestMethod]
        public void GetAllPermittedReturnsAllRepositoriesToSystemAdmin()
        {
            AddRepo("TestRepo1");
            AddRepo("TestRepo2");
            AddRepo("TestRepo3");

            CollectionAssert.AreEqual(new[] { "TestRepo1", "TestRepo2", "TestRepo3" },
                _service.GetAllPermittedRepositories(GetAdminId(), RepositoryAccessLevel.Pull).Select(r => r.Name).OrderBy(r => r).ToArray());
        }

        [TestMethod]
        public void AnonymousRepoIsPermittedToAnybodyToPull()
        {
            var repo = MakeRepo("Repo1");
            repo.AnonymousAccess = true;
            Assert.IsTrue(_repos.Create(repo));

            var anonymousUser = Guid.Empty;
            Assert.AreEqual("Repo1", _service.GetAllPermittedRepositories(anonymousUser, RepositoryAccessLevel.Pull).Single().Name);
        }

        [TestMethod]
        public void AnonymousRepoIsPermittedToNamedUserToPull()
        {
            // A named user should have at least as good access as an anonymous user
            var repo = MakeRepo("Repo1");
            repo.AnonymousAccess = true;
            Assert.IsTrue(_repos.Create(repo));

            var user = AddUser();
            Assert.AreEqual("Repo1", _service.GetAllPermittedRepositories(user.Id, RepositoryAccessLevel.Pull).Single().Name);
        }

        [TestMethod]
        public void RepositoryIsPermittedToUser()
        {
            var user = AddUser();
            var repoWithUser = MakeRepo("Repo1");
            repoWithUser.Users = new[] { user };
            Assert.IsTrue(_repos.Create(repoWithUser));
            AddRepo("Repo2");

            Assert.AreEqual("Repo1", _service.GetAllPermittedRepositories(user.Id, RepositoryAccessLevel.Pull).Single().Name);
            Assert.AreEqual("Repo1", _service.GetAllPermittedRepositories(user.Id, RepositoryAccessLevel.Push).Single().Name);
            Assert.IsFalse(_service.GetAllPermittedRepositories(user.Id, RepositoryAccessLevel.Administer).Any());
        }

        [TestMethod]
        public void NewRepositoryNotPermittedToAnonymousUser()
        {
            var user = AddUser();
            var repoWithUser = MakeRepo("Repo1");
            repoWithUser.Users = new[] { user };
            Assert.IsTrue(_repos.Create(repoWithUser));

            Assert.IsFalse(_service.GetAllPermittedRepositories(Guid.Empty, RepositoryAccessLevel.Pull).Any());
        }

        [TestMethod]
        public void RepositoryIsPermittedToRepoAdministrator()
        {
            var user = AddUser();
            var repoWithAdmin = MakeRepo("Repo1");
            repoWithAdmin.Administrators = new[] { user };
            Assert.IsTrue(_repos.Create(repoWithAdmin));
            AddRepo("Repo2");

            Assert.AreEqual("Repo1", _service.GetAllPermittedRepositories(user.Id, RepositoryAccessLevel.Administer).Single().Name);
        }


        [TestMethod]
        public void SystemAdministratorCanAlwaysCreateRepo()
        {
            Assert.IsTrue(_service.HasCreatePermission(GetAdminId()));
        }

        [TestMethod]
        public void NormalUserCannotCreateRepo()
        {
            var user = AddUser();
            Assert.IsFalse(_service.HasCreatePermission(user.Id));
        }

        [TestMethod]
        public void NamedUserCanCreateRepoWithGlobalOptionSet()
        {
            var user = AddUser();
            UserConfiguration.Current.AllowUserRepositoryCreation = true;
            Assert.IsTrue(_service.HasCreatePermission(user.Id));
        }

        [TestMethod]
        public void AnonUserCannotCreateRepoWithGlobalOptionSet()
        {
            UserConfiguration.Current.AllowUserRepositoryCreation = true;
            Assert.IsFalse(_service.HasCreatePermission(Guid.Empty));
        }


        /// <summary>
        /// A check-permission routine which runs checks by both name and Guid, and makes sure they agree
        /// </summary>
        private bool CheckPermission(Guid userId, Guid repoId, RepositoryAccessLevel level)
        {
            bool byGuid = _service.HasPermission(userId, repoId, level);
            bool byName = _service.HasPermission(userId, _repos.GetRepository(repoId).Name, level);
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