using System;
using System.Linq;
using System.Text;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests
{
    [TestClass]
    public class EFSqliteRepositoryRepositoryTest : EFRepositoryRepositoryTest
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
    public class EfSqlServerRepositoryRepositoryTest : EFRepositoryRepositoryTest
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

    public abstract class EFRepositoryRepositoryTest
    {
        protected IDatabaseTestConnection _connection;
        IRepositoryRepository _repo;

        [TestMethod]
        public void NewRepoIsEmpty()
        {
            Assert.AreEqual(0, _repo.GetAllRepositories().Count);
        }

        [TestMethod]
        public void RespositoryWithNoUsersCanBeAdded()
        {
            var newRepo = MakeRepo("Repo1");

            _repo.Create(newRepo);

            Assert.AreEqual("Repo1", _repo.GetAllRepositories().Single().Name);
        }

        [TestMethod]
        public void DuplicateRepoAddReturnsFalse()
        {
            Assert.IsTrue(_repo.Create(MakeRepo("Repo1")));
            Assert.IsFalse(_repo.Create(MakeRepo("Repo1")));
        }

        [TestMethod]
        public void RespositoryWithUsersCanBeAdded()
        {
            var newRepo = MakeRepo("Repo1");
            newRepo.Users = new [] { AddUserFred() };

            _repo.Create(newRepo);

            Assert.AreEqual("Fred Blogs", _repo.GetAllRepositories().Single().Users.Single().DisplayName);
        }

        [TestMethod]
        public void RespositoryWithAdministratorCanBeAdded()
        {
            var newRepo = MakeRepo("Repo1");
            newRepo.Administrators = new[] { AddUserFred() };

            _repo.Create(newRepo);

            Assert.AreEqual("Fred Blogs", _repo.GetAllRepositories().Single().Administrators.Single().DisplayName);
        }

        [TestMethod]
        public void RespositoriesAdministeredAreFound()
        {
            var administator = AddUserFred();

            var newRepo1 = MakeRepo("Repo1");
            newRepo1.Administrators = new[] { administator };
            _repo.Create(newRepo1);

            var newRepo2 = MakeRepo("Repo2");
            _repo.Create(newRepo2);

            // Only one repo is administered by our user
            Assert.AreEqual("Repo1", _repo.GetAdministratedRepositories(administator.Id).Single().Name);
        }

        [TestMethod]
        public void NewRepoCanBeRetrievedById()
        {
            var newRepo1 = MakeRepo("Repo1");
            _repo.Create(newRepo1);

            Assert.AreEqual("Repo1", _repo.GetRepository(newRepo1.Id).Name);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void NonExistentRepoIdThrowsException()
        {
            var newRepo1 = MakeRepo("Repo1");
            _repo.Create(newRepo1);

            _repo.GetRepository(Guid.NewGuid());
        }

        [TestMethod]
        public void NonExistentRepoNameReturnsNull()
        {
            var newRepo1 = MakeRepo("Repo1");
            _repo.Create(newRepo1);

            Assert.IsNull(_repo.GetRepository("Repo2"));
        }

        [TestMethod]
        public void NewRepoCanBeRetrievedByName()
        {
            var newRepo1 = MakeRepo("Repo1");
            _repo.Create(newRepo1);

            Assert.AreEqual("Repo1", _repo.GetRepository("Repo1").Name);
        }

        [TestMethod]
        public void NewRepoCanBeDeleted()
        {
            _repo.Create(MakeRepo("Repo1"));
            _repo.Create(MakeRepo("Repo2"));

            _repo.Delete(_repo.GetRepository("Repo1").Id);

            Assert.AreEqual("Repo2", _repo.GetAllRepositories().Single().Name);
        }

        [TestMethod]
        public void DeletingMissingRepoIsSilentlyIgnored()
        {
            _repo.Create(MakeRepo("Repo1"));

            _repo.Delete(Guid.NewGuid());

            Assert.AreEqual("Repo1", _repo.GetAllRepositories().Single().Name);
        }

        [TestMethod]
        public void RepoSimplePropertiesAreSavedOnUpdate()
        {
            var repo = MakeRepo("Repo1");
            _repo.Create(repo);

            repo.Name = "SonOfRepo";
            repo.Group = "RepoGroup";
            repo.AnonymousAccess = true;
            repo.AuditPushUser = true;
            repo.Description = "New desc";

            _repo.Update(repo);

            var readBackRepo = _repo.GetRepository("SonOfRepo");
            Assert.AreEqual("SonOfRepo", readBackRepo.Name);
            Assert.AreEqual(repo.Group, readBackRepo.Group);
            Assert.AreEqual(repo.AnonymousAccess, readBackRepo.AnonymousAccess);
            Assert.AreEqual(repo.AuditPushUser, readBackRepo.AuditPushUser);
            Assert.AreEqual(repo.Description, readBackRepo.Description);
        }

        [TestMethod]
        public void RepoLogoCanBeAddedWithUpdate()
        {
            var repo = MakeRepo("Repo1");
            _repo.Create(repo);

            var logoBytes = Encoding.UTF8.GetBytes("Hello");
            repo.Logo = logoBytes;

            _repo.Update(repo);

            var readBackRepo = _repo.GetRepository("Repo1");
            CollectionAssert.AreEqual(logoBytes, readBackRepo.Logo);
        }

        [TestMethod]
        public void RepoLogoCanBeRemovedWithUpdate()
        {
            var repo = MakeRepo("Repo1");
            _repo.Create(repo);

            repo.Logo = Encoding.UTF8.GetBytes("Hello");
            _repo.Update(repo);
            repo.RemoveLogo = true;
            _repo.Update(repo);

            Assert.IsNull(_repo.GetRepository("Repo1").Logo);
        }

        [TestMethod]
        public void NewRepositoryIsPermittedToNobody()
        {
            _repo.Create(MakeRepo("Repo1"));

            Assert.AreEqual(0, _repo.GetRepository("Repo1").Administrators.Length);
            Assert.AreEqual(0, _repo.GetRepository("Repo1").Teams.Length);
            Assert.AreEqual(0, _repo.GetRepository("Repo1").Users.Length);
        }

        [TestMethod]
        public void AnonymousRepoIsPermittedToAnybody()
        {
            var repo = MakeRepo("Repo1");
            repo.AnonymousAccess = true;
            _repo.Create(repo);

            var randomUserId = Guid.NewGuid();
            Assert.AreEqual("Repo1", _repo.GetPermittedRepositories(randomUserId, null).Single().Name);
        }

        [TestMethod]
        public void RepositoryIsPermittedToUser()
        {
            var repoWithUser = MakeRepo("Repo1");
            var user = AddUserFred();
            repoWithUser.Users = new[] { user };
            _repo.Create(repoWithUser);
            var repoWithoutUser = MakeRepo("Repo2");
            _repo.Create(repoWithoutUser);

            Assert.AreEqual("Repo1", _repo.GetPermittedRepositories(user.Id, null).Single().Name);
        }

        [TestMethod]
        public void NewRepositoryNotPermittedToUnknownUser()
        {
            var repoWithUser = MakeRepo("Repo1");
            var user = AddUserFred();
            repoWithUser.Users = new[] { user };
            _repo.Create(repoWithUser);

            var unknownUserId = Guid.NewGuid();
            Assert.IsFalse(_repo.GetPermittedRepositories(unknownUserId, null).Any());
        }

        [TestMethod]
        public void RepositoryIsPermittedToAdministrator()
        {
            var repoWithAdmin = MakeRepo("Repo1");
            var user = AddUserFred();
            repoWithAdmin.Administrators = new[] { user };
            _repo.Create(repoWithAdmin);
            var repoWithoutUser = MakeRepo("Repo2");
            _repo.Create(repoWithoutUser);

            Assert.AreEqual("Repo1", _repo.GetPermittedRepositories(user.Id, null).Single().Name);
        }

        [TestMethod]
        public void RepositoryIsPermittedToTeamEvenWhenUserIsNotInTeam()
        {
            var user = AddUserFred();
            var team = AddTeam();
            var repoWithTeam = MakeRepo("Repo1");
            repoWithTeam.Teams = new[] { team };
            _repo.Create(repoWithTeam);
            var repoWithoutTeam = MakeRepo("Repo2");
            _repo.Create(repoWithoutTeam);

            Assert.AreEqual("Repo1", _repo.GetPermittedRepositories(user.Id, new[] { team.Id }).Single().Name);
        }

        [TestMethod]
        public void RepositoryIsNotPermittedIfTeamIsWrong()
        {
            var user = AddUserFred();
            var team = AddTeam();
            var repoWithTeam = MakeRepo("Repo1");
            repoWithTeam.Teams = new[] { team };
            _repo.Create(repoWithTeam);
            var repoWithoutTeam = MakeRepo("Repo2");
            _repo.Create(repoWithoutTeam);

            Assert.AreEqual(0, _repo.GetPermittedRepositories(user.Id, new[] { Guid.NewGuid() }).Count);
        }

        [TestMethod]
        public void RepositoryIsReportedAsAccessibleToTeam()
        {
            var team = AddTeam();
            var repoWithTeam = MakeRepo("Repo1");
            repoWithTeam.Teams = new[] { team };
            _repo.Create(repoWithTeam);
            var repoWithoutTeam = MakeRepo("Repo2");
            _repo.Create(repoWithoutTeam);

            Assert.AreEqual("Repo1", _repo.GetTeamRepositories(new[] { team.Id }).Single().Name);
        }

        [TestMethod]
        public void NoReposistoriesListedIfNoneInTeam()
        {
            var team = AddTeam();
            var repoWithoutTeam1 = MakeRepo("Repo1");
            _repo.Create(repoWithoutTeam1);
            var repoWithoutTeam2 = MakeRepo("Repo2");
            _repo.Create(repoWithoutTeam2);

            Assert.AreEqual(0, _repo.GetTeamRepositories(new[] { team.Id }).Count);
        }

        UserModel AddUserFred()
        {
            EFMembershipService memberService = new EFMembershipService { CreateContext = GetContext };
            memberService.CreateUser("fred", "letmein", "Fred", "Blogs", "fred@aol");
            return memberService.GetUserModel("fred");
        }

        TeamModel AddTeam()
        {
            EFTeamRepository teams = new EFTeamRepository { CreateContext = GetContext };
            var newTeam = new TeamModel { Name="Team1" };
            teams.Create(newTeam);
            return newTeam;
        }

        private static RepositoryModel MakeRepo(string name)
        {
            var newRepo = new RepositoryModel();
            newRepo.Name = name;
            newRepo.Users = new UserModel[0];
            newRepo.Administrators = new UserModel[0];
            newRepo.Teams = new TeamModel[0];
            return newRepo;
        }

        BonoboGitServerContext GetContext()
        {
            return _connection.GetContext();
        }

        protected void InitialiseTestObjects()
        {
            _repo = new EFRepositoryRepository {CreateContext = () => _connection.GetContext()};
            new AutomaticUpdater().RunWithContext(_connection.GetContext());
        }
    }
}