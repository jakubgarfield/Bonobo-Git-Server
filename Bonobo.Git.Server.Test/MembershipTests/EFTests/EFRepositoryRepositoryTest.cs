﻿using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Bonobo.Git.Server.Test.MembershipTests.EFTests
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

    public abstract class EFRepositoryRepositoryTest : RepositoryRepositoryTestBase
    {
        protected IDatabaseTestConnection _connection;

        protected override UserModel AddUserFred()
        {
            IMembershipService memberService = new EFMembershipService(GetContext);
            memberService.CreateUser("fred", "letmein", "Fred", "Blogs", "fred@aol");
            return memberService.GetUserModel("fred");
        }

        protected override TeamModel AddTeam()
        {
            EFTeamRepository teams = new EFTeamRepository(GetContext);
            var newTeam = new TeamModel { Name="Team1" };
            teams.Create(newTeam);
            return newTeam;
        }

        BonoboGitServerContext GetContext()
        {
            return _connection.GetContext();
        }

        protected void InitialiseTestObjects()
        {
            _repo = new EFRepositoryRepository(() => _connection.GetContext());
            new AutomaticUpdater().RunWithContext(_connection.GetContext(), Substitute.For<IAuthenticationProvider>());
        }
    }
}