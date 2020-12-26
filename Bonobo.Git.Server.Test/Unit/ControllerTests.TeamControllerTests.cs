using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Test.Unit
{
    public partial class ControllerTests
    {
        [TestClass]
        public class TeamControllerTests : ControllerDependendencyBuilders
        {
            private static Mock<IRepositoryRepository> repositoryRepositoryMock;

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
                var teamRepositoryMock = SetupControllerContextAndTeamRepository();
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

            [TestMethod]
            public void Get_Edit_Executed_With_ControllerContext_Setup__Returns_ViewResult_With_Not_Null_Model()
            {
                // Arrange
                var teamRepositoryMock = SetupControllerContextAndTeamRepository();
                var membershipServiceMock = SetupMembershipServiceMock();
                SetupMembershipServiceMockToReturnAnEmptyListOfUsers(membershipServiceMock);

                Guid requestedGuid = Guid.NewGuid();
                SetupTeamRepositoryMockToReturnASpecificTeamWhenCallingGetTeamMethod(teamRepositoryMock, requestedGuid);

                TeamController teamController = SutAs<TeamController>();
                teamController.TeamRepository = teamRepositoryMock.Object;
                teamController.MembershipService = membershipServiceMock.Object;

                // Act
                var result = teamController.Edit(requestedGuid);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
                Assert.IsTrue(sut.ModelState.IsValid);

                var viewResult = result as ViewResult;
                Assert.IsNotNull(viewResult.Model);
                Assert.IsInstanceOfType(viewResult.Model, typeof(TeamEditModel));

                var teamEditModel = viewResult.Model as TeamEditModel;
                Assert.AreEqual(requestedGuid, teamEditModel.Id);
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
                var teamRepositoryMock = SetupControllerContextAndTeamRepository();
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
                var membershipServiceMock = SetupMembershipServiceMock();
                SetupMembershipServiceMockToReturnAnEmptyListOfUsers(membershipServiceMock);
                var teamRepositoryMock = SetupControllerContextAndTeamRepository();
                TeamController teamController = SutAs<TeamController>();
                teamController.TeamRepository = teamRepositoryMock.Object;
                teamController.MembershipService = membershipServiceMock.Object;

                Guid requestedGuid = Guid.NewGuid();

                TeamEditModel model = new TeamEditModel { Id = requestedGuid };
                SetupTeamRepositoryMockToReturnASpecificTeamWhenCallingGetTeamMethod(teamRepositoryMock, requestedGuid);

                // Act
                var result = teamController.Edit(model);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
                var viewResult = result as ViewResult;
                Assert.IsNotNull(viewResult.Model);
            }

            // get Create
            [TestMethod]
            public void Get_Create_Executed_Without_Arranging_TeamRepository__Throws_NullReferenceException()
            {
                // Arrange
                try
                {
                    // Act
                    SutAs<TeamController>().Create();
                }
                catch (NullReferenceException)
                {
                    return;
                }
                // Assert
                Assert.Fail();
            }

            [TestMethod]
            public void Get_Create_Executed_Arranging_MembershipService__Returns_ViewResult()
            {
                // Arrange
                var membershipServiceMock = SetupMembershipServiceMock();
                SetupMembershipServiceMockToReturnAnEmptyListOfUsers(membershipServiceMock);
                TeamController teamController = SutAs<TeamController>();
                teamController.MembershipService = membershipServiceMock.Object;

                // Act
                var result = teamController.Create();

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));

                var viewResult = result as ViewResult;
                Assert.IsNotNull(viewResult.Model);
                Assert.IsInstanceOfType(viewResult.Model, typeof(TeamEditModel));
            }

            // post Create
            [TestMethod]
            public void Post_Create_Executed_With_Null_ViewModel__Throws_NullReferenceException()
            {
                // Arrange
                try
                {
                    // Act
                    SutAs<TeamController>().Create(null);
                }
                catch (NullReferenceException)
                {
                    return;
                }
                // Assert
                Assert.Fail();
            }

            [TestMethod]
            public void Post_Create_Executed_With_NonNull_ViewModel_Without_Arranging_TeamRepository__Throws_NullReferenceException()
            {
                // Arrange
                try
                {
                    // Act
                    SutAs<TeamController>().Create(new TeamEditModel());
                }
                catch (NullReferenceException)
                {
                    return;
                }
                // Assert
                Assert.Fail();
            }

            [TestMethod]
            public void Post_Create_Executed_With_NonNull_Empty_ViewModel__Returns_ViewResult_And_Invalid_ModelState()
            {
                // Arrange
                var teamRepositoryMock = SetupControllerContextAndTeamRepository();
                TeamController teamController = SutAs<TeamController>();
                teamController.TeamRepository = teamRepositoryMock.Object;

                TeamEditModel model = new TeamEditModel();
                BindModelToController(model);

                // Act
                var result = teamController.Create(model);

                // Assert
                Assert.IsFalse(teamController.ModelState.IsValid);
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));

                var viewResult = result as ViewResult;
                Assert.IsNotNull(viewResult.Model);
            }

            [TestMethod]
            public void Post_Create_Executed_With_Valid_ViewModel_Arranging_TeamRepository__Throws_NullReferenceException()
            {
                // Arrange
                var teamRepositoryMock = SetupControllerContextAndTeamRepository();
                SetupTeamRepositoryToSucceedWhenCreatingATeam(teamRepositoryMock);
                TeamController teamController = SutAs<TeamController>();
                teamController.TeamRepository = teamRepositoryMock.Object;

                TeamEditModel model = new TeamEditModel
                {
                    Name = "name",
                };

                // Act
                var result = teamController.Create(model);

                // Assert
                Assert.AreEqual(true, teamController.TempData["CreateSuccess"]);
                AssertAndGetRedirectToRouteResult(result);
            }

            // get Delete
            [TestMethod]
            public void Get_Delete_Executed_Without_Arranging_TeamRepository_With_Empty_Id__Throws_NullReferenceException()
            {
                // Arrange
                try
                {
                    // Act
                    SutAs<TeamController>().Delete(Guid.Empty);
                }
                catch (NullReferenceException)
                {
                    return;
                }
                // Assert
                Assert.Fail();
            }

            [TestMethod]
            public void Get_Delete_Executed_Arranging_TeamRepository_With_No_Setup_For_GetTeam_With_Empty_Id__Returns_Null_Model()
            {
                // Arrange
                var teamRepositoryMock = SetupControllerContextAndTeamRepository();
                TeamController teamController = SutAs<TeamController>();
                teamController.TeamRepository = teamRepositoryMock.Object;

                // Act
                var result = teamController.Delete(Guid.Empty);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));

                var viewResult = result as ViewResult;
                Assert.IsNull(viewResult.Model);
            }

            [TestMethod]
            public void Get_Delete_Executed_Arranging_TeamRepository_With_Setting_Up_GetTeam_With_Valid_Id__Returns_NonNull_Model()
            {
                // Arrange
                var teamRepositoryMock = SetupControllerContextAndTeamRepository();
                var membershipServiceMock = SetupMembershipServiceMock();
                SetupMembershipServiceMockToReturnAnEmptyListOfUsers(membershipServiceMock);

                TeamController teamController = SutAs<TeamController>();
                teamController.TeamRepository = teamRepositoryMock.Object;
                teamController.MembershipService = membershipServiceMock.Object;

                var requestedId = Guid.NewGuid();
                SetupTeamRepositoryMockToReturnASpecificTeamWhenCallingGetTeamMethod(teamRepositoryMock, requestedId);

                // Act
                var result = teamController.Delete(requestedId);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));

                var viewResult = result as ViewResult;
                Assert.IsNotNull(viewResult.Model);
            }

            // post Delete
            [TestMethod]
            public void Post_Delete_Executed_Without_Arranging_TeamRepository_With_Null_Model__Returns_RedirectToActionResult_Without_TempData()
            {
                // Arrange

                // Act
                var result = SutAs<TeamController>().Delete(null);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));

                var redirectToRouteResult = result as RedirectToRouteResult;
                Assert.AreEqual(1, redirectToRouteResult.RouteValues.Count);
                Assert.AreEqual("Index", redirectToRouteResult.RouteValues["action"]);

                Assert.AreEqual(0, sut.TempData.Count);
            }

            [TestMethod]
            public void Post_Delete_Executed_Arranging_TeamRepository_With_Valid_Model__Returns_RedirectToActionResult_With_TempData()
            {
                // Arrange
                Guid requestedId = Guid.NewGuid();
                TeamController teamController = SutAs<TeamController>();
                var teamRepositoryMock = SetupControllerContextAndTeamRepository();
                teamController.TeamRepository = teamRepositoryMock.Object;
                SetupTeamRepositoryMockToReturnASpecificTeamWhenCallingGetTeamMethod(teamRepositoryMock, requestedId);

                // Act
                var result = teamController.Delete(new TeamEditModel { Id = requestedId });

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));

                var redirectToRouteResult = result as RedirectToRouteResult;
                Assert.AreEqual(1, redirectToRouteResult.RouteValues.Count);
                Assert.AreEqual("Index", redirectToRouteResult.RouteValues["action"]);

                Assert.AreEqual(1, sut.TempData.Count);
                Assert.AreEqual("DeleteSuccess", sut.TempData.Keys.FirstOrDefault());
                Assert.AreEqual(true, sut.TempData["DeleteSuccess"]);
            }

            // get Detail
            [TestMethod]
            public void Get_Detail_Without_Arranging_TeamRepository__Throws_NullReferenceException()
            {
                // Arrange

                // Act
                try
                {
                    SutAs<TeamController>().Detail(Guid.Empty);
                }
                catch (NullReferenceException)
                {
                    return;
                }

                // Assert
                Assert.Fail();
            }

            [TestMethod]
            public void Get_Detail_Arranging_TeamRepository_With_Unknown_Id__Returns_ViewResult_With_Null_Model()
            {
                // Arrange
                TeamController teamController = SutAs<TeamController>();
                var teamRepositoryMock = SetupControllerContextAndTeamRepository();
                teamController.TeamRepository = teamRepositoryMock.Object;

                // Act
                var result = teamController.Detail(Guid.Empty);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));

                var viewResult = result as ViewResult;
                Assert.IsNull(viewResult.Model);
            }

            [TestMethod]
            public void Get_Detail_Arranging_TeamRepository_With_Known_Id__Returns_ViewResult_With_Null_Model()
            {
                // Arrange
                Guid requestedId = Guid.NewGuid();
                TeamController teamController = SutAs<TeamController>();
                SetupRepositoryRepositoryToReturnAnEmptyListForASpecificId(requestedId);
                var teamRepositoryMock = SetupControllerContextAndTeamRepository();
                var membershipServiceMock = SetupMembershipServiceMock();
                SetupTeamRepositoryMockToReturnASpecificTeamWhenCallingGetTeamMethod(teamRepositoryMock, requestedId);
                teamController.TeamRepository = teamRepositoryMock.Object;
                teamController.MembershipService = membershipServiceMock.Object;
                teamController.RepositoryRepository = repositoryRepositoryMock.Object;

                // Act
                var result = teamController.Detail(requestedId);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));

                var viewResult = result as ViewResult;
                Assert.IsNotNull(viewResult.Model);
            }

            private void SetupRepositoryRepositoryToReturnAnEmptyListForASpecificId(Guid requestedId)
            {
                repositoryRepositoryMock = new Mock<IRepositoryRepository>();
                repositoryRepositoryMock.Setup(r => r.GetTeamRepositories(new[] { requestedId }))
                                        .Returns(new List<RepositoryModel>());
                
            }
        }
    }
}
