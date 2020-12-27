using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Bonobo.Git.Server.Test.Unit
{
    public partial class ControllerTests
    {
        [TestClass]
        public class RepositoryControllerTests : ControllerTests
        {
            [TestInitialize]
            public void TestInitialize()
            {
                sut = new RepositoryController();
            }

            // get Edit
            [TestMethod]
            public void Get_Edit_Executed_With_Empty_Id__Throws_NullReferenceException()
            {
                // Arrange
                try
                {
                    // Act
                    SutAs<RepositoryController>().Edit(Guid.Empty);
                }
                catch (NullReferenceException)
                {
                    return;
                }
                // Assert
                Assert.Fail();
            }

            // post Edit
            [TestMethod]
            public void Post_Edit_Executed_With_Random_id_returns_xx()
            {
                // arrange
                ArrangeUserConfiguration();
                var guid = Guid.NewGuid();

                var repositoryController = SutAs<RepositoryController>();
                var repositoryRepositoryMock = SetupMock<IRepositoryRepository>().SetupToReturnAModelWithASpecificIdWhenCallingGetRepositoryMethod(guid);
                var membershipServiceMock = SetupMock<IMembershipService>().SetupToReturnAnEmptyUserModelListWhenCallingGetAllUsers();                
                var teamRepositoryMock = SetupMock<ITeamRepository>();
                teamRepositoryMock.Setup(s => s.GetAllTeams())
                                  .Returns(new List<TeamModel> { });
                repositoryController.RepositoryRepository = repositoryRepositoryMock.Object;
                repositoryController.MembershipService = membershipServiceMock.Object;
                repositoryController.TeamRepository = teamRepositoryMock.Object;

                //act
                var result = repositoryController.Edit(guid);

                Assert.IsNotNull(result);
            }

            // get Create
            // post Create
            // get Delete
            // post Delete
            // get Clone
            // post Clone
        }
    }
}
