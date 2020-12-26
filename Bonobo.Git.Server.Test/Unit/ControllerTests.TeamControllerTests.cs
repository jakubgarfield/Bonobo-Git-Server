using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Test.Unit
{
    public partial class ControllerTests
    {
        [TestClass]
        public class TeamControllerTests : ControllerWithTeamRepositoryAndMembershipServiceTests
        {
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
            public void Get_Edit_Executed_With_ControllerContext_Setup__Returns_ViewResult_With_Null_Model()
            {
                // Arrange
                SetupControllerContextAndTeamRepository();
                TeamController teamController = SutAs<TeamController>();
                teamController.TeamRepository = teamRepositoryMock.Object;

                // Act
                var result = teamController.Edit(Guid.Empty);

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
                SetupControllerContextAndTeamRepository();

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
                SetupControllerContextAndTeamRepository();
                TeamController teamController = SutAs<TeamController>();
                teamController.TeamRepository = teamRepositoryMock.Object;

                TeamEditModel model = new TeamEditModel();
                BindModelToController(model);

                // Act
                var result = teamController.Edit(model);

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
                SetupMembershipServiceMock();
                SetupMembershipServiceMockToReturnAnEmptyListOfUsers();
                SetupControllerContextAndTeamRepository();
                TeamController teamController = SutAs<TeamController>();
                teamController.TeamRepository = teamRepositoryMock.Object;
                teamController.MembershipService = membershipServiceMock.Object;

                Guid requestedGuid = Guid.NewGuid();

                TeamEditModel model = new TeamEditModel { Id = requestedGuid };
                SetupTeamRepositoryMockToReturnASpecificTeamWhenCallingGetTeamMethod(requestedGuid);

                // Act
                var result = teamController.Edit(model);

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
        }
    }
}
