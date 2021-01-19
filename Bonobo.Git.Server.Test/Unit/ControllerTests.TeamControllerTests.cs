using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Bonobo.Git.Server.Test.Unit
{
    public partial class ControllerTests
    {
        [TestClass]
        public class TeamControllerTests : ControllerTests
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

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<TeamController>().Edit(Guid.Empty));
            }

            [TestMethod]
            public void Get_Edit_Executed_With_ControllerContext_Setup__Returns_ViewResult_With_Null_Model()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();
                var teamRepositoryMock = SetupMock<ITeamRepository>();
                var teamController = SutAs<TeamController>();
                teamController.TeamRepository = teamRepositoryMock.Object;

                // Act
                var result = teamController.Edit(Guid.Empty);

                // Assert
                Assert.IsTrue(sut.ModelState.IsValid);
                var viewResult = AssertAndGetViewResult(result);

                Assert.IsNull(viewResult.Model);
            }

            [TestMethod]
            public void Get_Edit_Executed_With_ControllerContext_Setup__Returns_ViewResult_With_Not_Null_Model()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();
                var requestedGuid = Guid.NewGuid();
                var teamRepositoryMock = SetupMock<ITeamRepository>().SetupToReturnASpecificTeamWhenCallingGetTeamMethod(requestedGuid);
                var membershipServiceMock = SetupMock<IMembershipService>().SetupToReturnAnEmptyUserModelListWhenCallingGetAllUsers();

                var teamController = SutAs<TeamController>();
                teamController.TeamRepository = teamRepositoryMock.Object;
                teamController.MembershipService = membershipServiceMock.Object;

                // Act
                var result = teamController.Edit(requestedGuid);

                // Assert
                Assert.IsTrue(sut.ModelState.IsValid);
                var viewResult = AssertAndGetViewResult(result);

                Assert.IsNotNull(viewResult.Model);
                Assert.IsInstanceOfType(viewResult.Model, typeof(TeamEditModel));

                var teamEditModel = viewResult.Model as TeamEditModel;
                Assert.AreEqual(requestedGuid, teamEditModel.Id);
            }

            // post Edit
            [TestMethod]
            public void Post_Edit_Executed_With_Null_Model_And_Without_ControllerContext_Setup__Throws_NullReferenceException()
            {
                // Arrange

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<TeamController>().Edit(default(TeamEditModel)));
            }

            [TestMethod]
            public void Post_Edit_Executed_With_Bare_Model_And_Without_ControllerContext_Setup__Throws_NullReferenceException()
            {
                // Arrange

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<TeamController>().Edit(new TeamEditModel()));
            }

            [TestMethod]
            public void Post_Edit_Executed_With_Null_Model_And_With_ControllerContext_Setup__Throws_NullReferenceException()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<TeamController>().Edit(null));
            }

            [TestMethod]
            public void Post_Edit_Executed_With_NonExistent_Model_And_With_ControllerContext_Setup__Returns_ViewResult()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();
                var teamRepositoryMock = SetupMock<ITeamRepository>();
                var teamController = SutAs<TeamController>();
                teamController.TeamRepository = teamRepositoryMock.Object;

                var model = new TeamEditModel();
                BindModelToController(model);

                // Act
                var result = teamController.Edit(model);

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsNull(viewResult.Model);
            }

            [TestMethod]
            public void Post_Edit_Executed_With_Existent_Model_And_With_ControllerContext_Setup__Returns_ViewResult_With_Model()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();
                var requestedGuid = Guid.NewGuid();
                var membershipServiceMock = SetupMock<IMembershipService>().SetupToReturnAnEmptyUserModelListWhenCallingGetAllUsers();
                var teamRepositoryMock = SetupMock<ITeamRepository>().SetupToReturnASpecificTeamWhenCallingGetTeamMethod(requestedGuid);

                var teamController = SutAs<TeamController>();
                teamController.TeamRepository = teamRepositoryMock.Object;
                teamController.MembershipService = membershipServiceMock.Object;

                var model = new TeamEditModel { Id = requestedGuid };

                // Act
                var result = teamController.Edit(model);

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsNotNull(viewResult.Model);
            }

            // get Create
            [TestMethod]
            public void Get_Create_Executed_Without_Arranging_TeamRepository__Throws_NullReferenceException()
            {
                // Arrange

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<TeamController>().Create());
            }

            [TestMethod]
            public void Get_Create_Executed_Arranging_MembershipService__Returns_ViewResult()
            {
                // Arrange
                var membershipServiceMock = SetupMock<IMembershipService>().SetupToReturnAnEmptyUserModelListWhenCallingGetAllUsers();
                var teamController = SutAs<TeamController>();
                teamController.MembershipService = membershipServiceMock.Object;

                // Act
                var result = teamController.Create();

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsNotNull(viewResult.Model);
                Assert.IsInstanceOfType(viewResult.Model, typeof(TeamEditModel));
            }

            // post Create
            [TestMethod]
            public void Post_Create_Executed_With_Null_ViewModel__Throws_NullReferenceException()
            {
                // Arrange

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<TeamController>().Create(default(TeamEditModel)));
            }

            [TestMethod]
            public void Post_Create_Executed_With_NonNull_ViewModel_Without_Arranging_TeamRepository__Throws_NullReferenceException()
            {
                // Arrange

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<TeamController>().Create(new TeamEditModel()));
            }

            [TestMethod]
            public void Post_Create_Executed_With_Invalid_ViewModel__Returns_ViewResult_And_Invalid_ModelState()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();
                var teamRepositoryMock = SetupMock<ITeamRepository>();
                var teamController = SutAs<TeamController>();
                teamController.TeamRepository = teamRepositoryMock.Object;

                var model = new TeamEditModel();
                BindModelToController(model);

                // Act
                var result = teamController.Create(model);

                // Assert
                Assert.IsFalse(teamController.ModelState.IsValid);
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsNotNull(viewResult.Model);
            }

            [TestMethod]
            public void Post_Create_Executed_With_Valid_ViewModel_Arranging_TeamRepository__Returns_RedirectToViewResult_With_TempData_Flag_True_And_ModelId()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();
                var teamRepositoryMock = SetupMock<ITeamRepository>().SetupToSucceedWhenCreatingATeam();
                var teamController = SutAs<TeamController>();
                teamController.TeamRepository = teamRepositoryMock.Object;

                var model = new TeamEditModel
                {
                    Name = "name",
                    Id = Guid.NewGuid()
                };

                // Act
                var result = teamController.Create(model);

                // Assert
                Assert.AreEqual(true, teamController.TempData["CreateSuccess"]);
                var redirectToRouteResult = AssertAndGetRedirectToRouteResult(result);
                Assert.AreEqual(1, redirectToRouteResult.RouteValues.Count);
                Assert.AreEqual("action", redirectToRouteResult.RouteValues.Keys.First());
                Assert.AreEqual("Index", redirectToRouteResult.RouteValues["action"]);
                Assert.AreEqual(2, sut.TempData.Count);
                Assert.AreEqual(true, sut.TempData["CreateSuccess"]);
                Assert.AreEqual(model.Id, sut.TempData["NewTeamId"]);
            }

            // get Delete
            [TestMethod]
            public void Get_Delete_Executed_Without_Arranging_TeamRepository_With_Empty_Id__Throws_NullReferenceException()
            {
                // Arrange

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<TeamController>().Delete(Guid.Empty));
            }

            [TestMethod]
            public void Get_Delete_Executed_Arranging_TeamRepository_With_No_Setup_For_GetTeam_With_Empty_Id__Returns_Null_Model()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();
                var teamRepositoryMock = SetupMock<ITeamRepository>();
                var teamController = SutAs<TeamController>();
                teamController.TeamRepository = teamRepositoryMock.Object;

                // Act
                var result = teamController.Delete(Guid.Empty);

                // Assert
                var viewResult = AssertAndGetViewResult(result);

                Assert.IsNull(viewResult.Model);
            }

            [TestMethod]
            public void Get_Delete_Executed_Arranging_TeamRepository_With_Setting_Up_GetTeam_With_Valid_Id__Returns_NonNull_Model()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();
                var requestedId = Guid.NewGuid();
                var teamRepositoryMock = SetupMock<ITeamRepository>().SetupToReturnASpecificTeamWhenCallingGetTeamMethod(requestedId);
                var membershipServiceMock = SetupMock<IMembershipService>().SetupToReturnAnEmptyUserModelListWhenCallingGetAllUsers();

                var teamController = SutAs<TeamController>();
                teamController.TeamRepository = teamRepositoryMock.Object;
                teamController.MembershipService = membershipServiceMock.Object;

                // Act
                var result = teamController.Delete(requestedId);

                // Assert
                var viewResult = AssertAndGetViewResult(result);
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

                var redirectToRouteResult = AssertAndGetRedirectToRouteResult(result);
                Assert.AreEqual(1, redirectToRouteResult.RouteValues.Count);
                Assert.AreEqual("Index", redirectToRouteResult.RouteValues["action"]);

                Assert.AreEqual(0, sut.TempData.Count);
            }

            [TestMethod]
            public void Post_Delete_Executed_Arranging_TeamRepository_With_Valid_Model__Returns_RedirectToActionResult_With_TempData()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();
                var requestedId = Guid.NewGuid();
                var teamController = SutAs<TeamController>();
                var teamRepositoryMock = SetupMock<ITeamRepository>().SetupToReturnASpecificTeamWhenCallingGetTeamMethod(requestedId);
                teamController.TeamRepository = teamRepositoryMock.Object;

                // Act
                var result = teamController.Delete(new TeamEditModel { Id = requestedId });

                // Assert
                var redirectToRouteResult = AssertAndGetRedirectToRouteResult(result);

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

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<TeamController>().Detail(Guid.Empty));
            }

            [TestMethod]
            public void Get_Detail_Arranging_TeamRepository_With_Unknown_Id__Returns_ViewResult_With_Null_Model()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();
                var teamController = SutAs<TeamController>();
                var teamRepositoryMock = SetupMock<ITeamRepository>();
                teamController.TeamRepository = teamRepositoryMock.Object;

                // Act
                var result = teamController.Detail(Guid.Empty);

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsNull(viewResult.Model);
            }

            [TestMethod]
            public void Get_Detail_Arranging_TeamRepository_With_Known_Id__Returns_ViewResult_With_Null_Model()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();
                var requestedId = Guid.NewGuid();
                var teamController = SutAs<TeamController>();
                var teamRepositoryMock = SetupMock<ITeamRepository>().SetupToReturnASpecificTeamWhenCallingGetTeamMethod(requestedId);
                var membershipServiceMock = SetupMock<IMembershipService>();
                var repositoryRepositoryMock = SetupMock<IRepositoryRepository>().SetupToReturnAnEmptyListForASpecificIdWhenCallingGetTeamRepositories(requestedId);

                teamController.TeamRepository = teamRepositoryMock.Object;
                teamController.MembershipService = membershipServiceMock.Object;
                teamController.RepositoryRepository = repositoryRepositoryMock.Object;

                // Act
                var result = teamController.Detail(requestedId);

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsNotNull(viewResult.Model);
            }
        }
    }
}
