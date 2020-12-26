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
        public class RepositoryControllerTests : ControllerDependendencyBuilders
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

                var sut = SutAs<RepositoryController>();
                var repositoryRepositoryMock = new Mock<IRepositoryRepository>();
                repositoryRepositoryMock.Setup(s => s.GetRepository(guid))
                                        .Returns(new RepositoryModel
                                        {
                                            Id = guid,
                                            Administrators = new UserModel[0],
                                            Name = "name"
                                        });
                var membershipServiceMock = new Mock<IMembershipService>();
                membershipServiceMock.Setup(s => s.GetAllUsers())
                                     .Returns(new List<UserModel> { });
                var teamRepositoryMock = new Mock<ITeamRepository>();
                teamRepositoryMock.Setup(s => s.GetAllTeams())
                                  .Returns(new List<TeamModel> { });
                sut.RepositoryRepository = repositoryRepositoryMock.Object;
                sut.MembershipService = membershipServiceMock.Object;
                sut.TeamRepository = teamRepositoryMock.Object;

                //act
                var result = sut.Edit(guid);

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
