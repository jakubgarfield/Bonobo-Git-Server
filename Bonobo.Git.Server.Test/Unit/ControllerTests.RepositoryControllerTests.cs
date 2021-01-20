using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            // get Index
            [TestMethod]
            public void Get_Index_Called_Without_Arrange__Throws_NullReferenceException()
            {
                // Arrange

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<RepositoryController>().Index());
            }

            [TestMethod]
            public void Get_Index_Called_Arranging_RepositoryPermissionService__Returns_ViewResult_With_Empty_Model()
            {
                // Arrange
                var repositoryController = SutAs<RepositoryController>();
                repositoryController.RepositoryPermissionService = SetupMock<IRepositoryPermissionService>().Object;

                // Act
                var result = repositoryController.Index();

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsInstanceOfType(viewResult.Model, typeof(Dictionary<string, RepositoryDetailModel[]>));
                var model = viewResult.Model as Dictionary<string, RepositoryDetailModel[]>;
                Assert.AreEqual(0, model.Count);
            }

            [TestMethod]
            public void Get_Index_Called_Arranging_RepositoryPermissionService_With__Returns()
            {
                // Arrange
                ArrangeUserConfiguration();
                sut.ControllerContext = CreateControllerContext();
                var repositoryController = SutAs<RepositoryController>();
                repositoryController.RepositoryPermissionService = SetupMock<IRepositoryPermissionService>().SetupGetAllPermittedRepositoriesToReturnAList(new List<RepositoryModel>
                                                                                                                                                            { new RepositoryModel
                                                                                                                                                                {
                                                                                                                                                                    Administrators = new UserModel[0],
                                                                                                                                                                    Name = "name"
                                                                                                                                                                }
                                                                                                                                                            })
                                                                                                            .Object;
                // Act
                var result = repositoryController.Index(searchString: "searchString");

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsInstanceOfType(viewResult.Model, typeof(Dictionary<string, RepositoryDetailModel[]>));
                var model = viewResult.Model as Dictionary<string, RepositoryDetailModel[]>;
                Assert.AreEqual(0, model.Count);
            }

            // get Edit
            [TestMethod]
            public void Get_Edit_Executed_With_Empty_Id__Throws_NullReferenceException()
            {
                // Arrange

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<RepositoryController>().Edit(Guid.Empty));
            }

            // post Edit
            [TestMethod]
            public void Post_Edit_Executed_With_Random_Id__Returns_ViewResult_With_Model_And_Data()
            {
                // Arrange
                ArrangeUserConfiguration();
                var guid = Guid.NewGuid();
                var repositoryController = SutAs<RepositoryController>();

                repositoryController.RepositoryRepository = SetupMock<IRepositoryRepository>().SetupToReturnAModelWithASpecificIdWhenCallingGetRepositoryMethod(guid)
                                                                                              .Object;
                repositoryController.MembershipService = SetupMock<IMembershipService>().SetupToReturnAnEmptyUserModelListWhenCallingGetAllUsers()
                                                                                        .Object;
                repositoryController.TeamRepository = SetupMock<ITeamRepository>().SetupToGetAllTeamsReturnAListOfTeams(new List<TeamModel> { })
                                                                                  .Object;

                // Act
                var result = repositoryController.Edit(guid);

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsNotNull(viewResult.Model);
                Assert.IsInstanceOfType(viewResult.Model, typeof(RepositoryDetailModel));
                var repositoryDetailModel = viewResult.Model as RepositoryDetailModel;
                Assert.AreEqual("name", repositoryDetailModel.Name);
                Assert.AreEqual(guid, repositoryDetailModel.Id);
            }

            // get Create
            [TestMethod]
            public void Get_Create_Executed_Arranging_RepositoryPermissionService__Returns_RedirectToRouteResult()
            {
                // Arrange
                var repositoryController = SutAs<RepositoryController>();
                repositoryController.RepositoryPermissionService = SetupMock<IRepositoryPermissionService>().SetupGetAllPermittedRepositoriesToReturnAList(new List<RepositoryModel>
                                                                                                                                                            { new RepositoryModel
                                                                                                                                                                {
                                                                                                                                                                    Administrators = new UserModel[0],
                                                                                                                                                                    Name = "name"
                                                                                                                                                                }
                                                                                                                                                            })
                                                                                                            .Object;

                // Act
                var result = repositoryController.Create();

                // Assert
                var redirectToRouteResult = AssertAndGetRedirectToRouteResult(result);
                Assert.AreEqual(2, redirectToRouteResult.RouteValues.Count);
                var routeValuesEnumerator = redirectToRouteResult.RouteValues.GetEnumerator();
                try
                {
                    routeValuesEnumerator.MoveNext();
                    Assert.AreEqual("action", routeValuesEnumerator.Current.Key);
                    Assert.AreEqual("Unauthorized", routeValuesEnumerator.Current.Value);
                    routeValuesEnumerator.MoveNext();
                    Assert.AreEqual("controller", routeValuesEnumerator.Current.Key);
                    Assert.AreEqual("Home", routeValuesEnumerator.Current.Value);
                }
                finally
                {
                    routeValuesEnumerator.Dispose();
                }
            }

            [TestMethod]
            public void Get_Create_Executed_Arranging_Dependencies_And_User_Has_Create_Permissions__Returns_ViewResult()
            {
                // Arrange
                var id = Guid.NewGuid();
                SetHttpContextMockIntoSUT(id);

                var repositoryController = SutAs<RepositoryController>();
                repositoryController.RepositoryPermissionService = SetupMock<IRepositoryPermissionService>().SetupHasCreatePermissionToReturnTrue(id)
                                                                                                            .Object;
                repositoryController.MembershipService = SetupMock<IMembershipService>().SetupToReturnAnEmptyUserModelListWhenCallingGetAllUsers()
                                                                                        .SetupToReturnARequestedUserModelById(id)
                                                                                        .Object;
                repositoryController.TeamRepository = SetupMock<ITeamRepository>().SetupToGetAllTeamsReturnAListOfTeams(new List<TeamModel>())
                                                                                  .Object;

                // Act
                var result = repositoryController.Create();

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsNotNull(viewResult.Model);
                Assert.IsInstanceOfType(viewResult.Model, typeof(RepositoryDetailModel));

                var repositoryDetailModel = viewResult.Model as RepositoryDetailModel;
                Assert.AreEqual(1, repositoryDetailModel.Administrators.Length);
                Assert.AreEqual(id, repositoryDetailModel.Administrators[0].Id);
            }

            // post Create
            [TestMethod]
            public void Post_Create_Executed_Without_Arranging_RepositoryPermissionService__Throws_NullReferenceException()
            {
                // Arrange

                // Act
                Assert.ThrowsException<NullReferenceException>(() => SutAs<RepositoryController>().Create(default(RepositoryDetailModel)));
            }

            [TestMethod]
            public void Post_Create_Executed_Arranging_RepositoryPermissionService_With_Default_Model__Returns_RedirectToRouteView()
            {
                // Arrange
                var repositoryController = SutAs<RepositoryController>();
                repositoryController.RepositoryPermissionService = SetupMock<IRepositoryPermissionService>().Object;

                // Act
                var result = repositoryController.Create(default(RepositoryDetailModel));

                // Assert
                var redirectToRouteResult = AssertAndGetRedirectToRouteResult(result);
                Assert.AreEqual(2, redirectToRouteResult.RouteValues.Count);
                var routeValuesEnumerator = redirectToRouteResult.RouteValues.GetEnumerator();

                try
                {
                    routeValuesEnumerator.MoveNext();
                    Assert.AreEqual("action", routeValuesEnumerator.Current.Key);
                    Assert.AreEqual("Unauthorized", routeValuesEnumerator.Current.Value);
                    routeValuesEnumerator.MoveNext();
                    Assert.AreEqual("controller", routeValuesEnumerator.Current.Key);
                    Assert.AreEqual("Home", routeValuesEnumerator.Current.Value);
                }
                finally
                {
                    routeValuesEnumerator.Dispose();
                }
            }

            [TestMethod]
            public void Post_Create_Executed_Arranging_RepositoryPermissionService_With_Default_Model_When_User_Has_Create_Permissions_Not_Mocking_RepositoryRepository_Not_Binding_Model_To_Controller__Throws_NullReferenceException()
            {
                // Arrange
                var id = Guid.NewGuid();
                SetHttpContextMockIntoSUT(id);

                var repositoryController = SutAs<RepositoryController>();
                repositoryController.RepositoryPermissionService = SetupMock<IRepositoryPermissionService>().SetupHasCreatePermissionToReturnTrue(id)
                                                                                                            .Object;
                var model = default(RepositoryDetailModel);

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => repositoryController.Create(model));
            }

            [TestMethod]
            public void Post_Create_Executed_Arranging_RepositoryPermissionService_With_Default_Model_When_User_Has_Create_Permissions_Not_Mocking_RepositoryRepository_Binding_Model_To_Controller__Throws_NullReferenceException()
            {
                // Arrange
                var id = Guid.NewGuid();
                SetHttpContextMockIntoSUT(id);

                var repositoryController = SutAs<RepositoryController>();
                repositoryController.RepositoryPermissionService = SetupMock<IRepositoryPermissionService>().SetupHasCreatePermissionToReturnTrue(id)
                                                                                                            .Object;
                var model = default(RepositoryDetailModel);
                BindModelToController(model);

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => repositoryController.Create(model));
            }

            [TestMethod]
            public void Post_Create_Executed_Arranging_RepositoryPermissionService_With_Empty_Model_When_User_Has_Create_Permissions_Not_Mocking_RepositoryRepository_Binding_Model_To_Controller__Throws_NullReferenceException()
            {
                // Arrange
                var id = Guid.NewGuid();
                SetHttpContextMockIntoSUT(id);

                var repositoryController = SutAs<RepositoryController>();
                repositoryController.RepositoryPermissionService = SetupMock<IRepositoryPermissionService>().SetupHasCreatePermissionToReturnTrue(id)
                                                                                                            .Object;
                var model = new RepositoryDetailModel();
                BindModelToController(model);

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => repositoryController.Create(model));
            }

            [TestMethod]
            public void Post_Create_Executed_Arranging_RepositoryPermissionService_With_Empty_Model_When_User_Has_Create_Permissions_Not_Mocking_RepositoryRepository_Binding_Model_To_Controller_Mocking_MembershipService__Throws_NullReferenceException()
            {
                // Arrange
                var id = Guid.NewGuid();
                SetHttpContextMockIntoSUT(id);

                var repositoryController = SutAs<RepositoryController>();
                repositoryController.RepositoryPermissionService = SetupMock<IRepositoryPermissionService>().SetupHasCreatePermissionToReturnTrue(id)
                                                                                                            .Object;
                repositoryController.MembershipService = SetupMock<IMembershipService>().SetupToReturnAnEmptyUserModelListWhenCallingGetAllUsers()
                                                                                        .Object;

                var model = new RepositoryDetailModel();
                BindModelToController(model);

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => repositoryController.Create(model));
            }

            [TestMethod]
            // this test randomly fails in marked line
            public void Post_Create_Executed_Arranging_RepositoryPermissionService_With_Empty_Model_When_User_Has_Create_Permissions_Not_Mocking_RepositoryRepository_Binding_Model_To_Controller_Mocking_MembershipService_And_TeamsRepository__Returns_ViewResult()
            {
                // Arrange
                var id = Guid.NewGuid();
                SetHttpContextMockIntoSUT(id);

                var repositoryController = SutAs<RepositoryController>();
                repositoryController.RepositoryPermissionService = SetupMock<IRepositoryPermissionService>().SetupHasCreatePermissionToReturnTrue(id)
                                                                                                            .Object;
                repositoryController.MembershipService = SetupMock<IMembershipService>().SetupToReturnAnEmptyUserModelListWhenCallingGetAllUsers()
                                                                                        .Object;
                repositoryController.TeamRepository = SetupMock<ITeamRepository>().SetupToGetAllTeamsReturnAListOfTeams(new List<TeamModel>())
                                                                                  .Object;

                var model = new RepositoryDetailModel();
                BindModelToController(model);

                // Act
                var result = repositoryController.Create(model);

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsNotNull(viewResult.Model);
                Assert.IsInstanceOfType(viewResult.Model, typeof(RepositoryDetailModel));
                var repositoryDetailModel = viewResult.Model as RepositoryDetailModel;

                Assert.IsFalse(repositoryController.ModelState.IsValid);
                Assert.AreEqual(1, repositoryController.ModelState.Count);
                using (var modelStateEnumerator = repositoryController.ModelState.GetEnumerator())
                {
                    modelStateEnumerator.MoveNext();
                    Assert.AreEqual("Name", modelStateEnumerator.Current.Key);
                    Assert.AreEqual(3, modelStateEnumerator.Current.Value.Errors.Count);
                    using (var modelStateErrorEnumerator = modelStateEnumerator.Current.Value.Errors.GetEnumerator())
                    {
                        // this test randomly fails here need to investigate why
                        AssertNextErrorMessageIs(modelStateErrorEnumerator, "empty repo name?", $"repository name '{repositoryDetailModel.Name ?? "<null>"}'");
                        AssertNextErrorMessageIs(modelStateErrorEnumerator, "\"Name\" contains characters that can't be in a file or directory name.");
                        AssertNextErrorMessageIs(modelStateErrorEnumerator, "\"Name\" shouldn't contain only whitespace characters.");
                    }
                }
                Assert.IsNotNull(repositoryDetailModel.AllAdministrators);
                Assert.IsNotNull(repositoryDetailModel.AllUsers);
                Assert.IsNotNull(repositoryDetailModel.AllTeams);
                Assert.IsNull(repositoryDetailModel.Users);
                Assert.IsNull(repositoryDetailModel.Teams);
                Assert.IsNull(repositoryDetailModel.Administrators);
                Assert.IsNotNull(repositoryDetailModel.PostedSelectedAdministrators);
                Assert.IsNotNull(repositoryDetailModel.PostedSelectedTeams);
                Assert.IsNotNull(repositoryDetailModel.PostedSelectedUsers);
            }

            // get Delete
            // post Delete
            // get Clone
            // post Clone
        }
    }
}
