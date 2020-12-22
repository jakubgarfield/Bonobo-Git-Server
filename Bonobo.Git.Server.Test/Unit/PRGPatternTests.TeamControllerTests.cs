using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Test.Unit
{
    public partial class PRGPatternTests
    {
        [TestClass]
        public class TeamControllerTests : PRGPatternTests
        {
            private Mock<IMembershipService> membershipServiceMock;
            private Mock<ITeamRepository> teamRepositoryMock;

            [TestInitialize]
            public void TestInitialize()
            {
                sut = new TeamController();
            }

            // get Edit
            [TestMethod]
            public void Get_Edit_Executed_Without_ControllerContext_Setup__Throws_NullReferenceException()
            {
                // Arrange
                try
                {
                    // Act
                    SutAs<TeamController>().Edit(Guid.Empty);
                }
                catch (NullReferenceException)
                {
                    return;
                }
                // Assert
                Assert.Fail();
            }

            [TestMethod]
            public void Get_Edit_Executed_With_ControllerContext_Setup__Returns_ViewResult()
            {
                // Arrange
                ArrangeControllerContextAndTeamRepository();

                // Act
                var result = SutAs<TeamController>().Edit(Guid.Empty);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
                Assert.IsTrue(sut.ModelState.IsValid);

                var viewResult = result as ViewResult;
                Assert.IsNull(viewResult.Model);
            }

            // post Edit
            [TestMethod]
            public void Post_Edit_Executed_With_Null_Model_And_Without_ControllerContext_Setup__Throws_NullReferenceException()
            {
                try
                {
                    SutAs<TeamController>().Edit(null);
                }
                catch (NullReferenceException)
                {
                    return;
                }
                Assert.Fail();
            }

            [TestMethod]
            public void Post_Edit_Executed_With_Bare_Model_And_Without_ControllerContext_Setup__Throws_NullReferenceException()
            {
                // Arrange
                try
                {
                    // Act
                    SutAs<TeamController>().Edit(new TeamEditModel());

                }
                catch (NullReferenceException)
                {
                    return;
                }
                // Assert
                Assert.Fail();
            }

            [TestMethod]
            public void Post_Edit_Executed_With_Null_Model_And_With_ControllerContext_Setup__Throws_NullReferenceException()
            {
                ArrangeControllerContextAndTeamRepository();

                try
                {
                    SutAs<TeamController>().Edit(null);
                }
                catch (NullReferenceException)
                {
                    return;
                }
                Assert.Fail();
            }

            [TestMethod]
            public void Post_Edit_Executed_With_NonExistent_Model_And_With_ControllerContext_Setup__Returns_ViewResult()
            {
                // Arrange
                ArrangeControllerContextAndTeamRepository();
                TeamEditModel model = new TeamEditModel();
                BindModelToController(model);

                // Act
                var result = SutAs<TeamController>().Edit(model);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
                var viewResult = result as ViewResult;
                Assert.IsNull(viewResult.Model);
            }

            [TestMethod]
            public void Post_Edit_Executed_With_Existent_Model_And_With_ControllerContext_Setup__Returns_ViewResult_With_Model()
            {
                // Arrange
                ArrangeControllerContextAndTeamRepository();
                SetupMembershipServiceMockIntoSUT();
                Guid requestedGuid = Guid.NewGuid();

                TeamEditModel model = new TeamEditModel { Id = requestedGuid };
                teamRepositoryMock.Setup(t => t.GetTeam(requestedGuid))
                                  .Returns(new TeamModel
                                  {
                                      Id = requestedGuid,
                                      Members = new UserModel[0]
                                  });
                membershipServiceMock.Setup(m => m.GetAllUsers())
                                     .Returns(new List<UserModel>());

                // Act
                var result = SutAs<TeamController>().Edit(model);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
                var viewResult = result as ViewResult;
                Assert.IsNotNull(viewResult.Model);
            }

            // get Create
            // post Create
            // get Delete
            // post Delete

            private void SetupMembershipServiceMockIntoSUT()
            {
                membershipServiceMock = new Mock<IMembershipService>();
                SutAs<TeamController>().MembershipService = membershipServiceMock.Object;
            }

            private void ArrangeControllerContextAndTeamRepository()
            {
                sut.ControllerContext = CreateControllerContext();
                teamRepositoryMock = new Mock<ITeamRepository>();
                SutAs<TeamController>().TeamRepository = teamRepositoryMock.Object;
            }
        }
    }
}
