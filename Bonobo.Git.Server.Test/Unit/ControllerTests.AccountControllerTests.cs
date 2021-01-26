using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Configuration;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Test.Unit
{
    public partial class ControllerTests
    {
        [TestClass]
        public class AccountControllerTests : ControllerTests
        {
            [TestInitialize]
            public void TestInitialize()
            {
                sut = new AccountController();
            }

            // get Delete
            [TestMethod]
            public void Get_Delete_Executed_With_Empty_Id_Without_Setting_Mocks__Throws_NullReferenceException()
            {
                // Arrange

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<AccountController>().Delete(Guid.Empty));
            }

            [TestMethod]
            public void Get_Delete_Executed_With_Empty_Id_Setting_Mock_To_MembershipService__Returns_RedirectToRouteResult()
            {
                // Arrange
                var accountController = SutAs<AccountController>();
                accountController.MembershipService = SetupMock<IMembershipService>().Object;

                // Act
                var result = accountController.Delete(Guid.Empty);

                // Assert
                var redirectToRouteResult = AssertAndGetRedirectToRouteResult(result);
                Assert.AreEqual("", redirectToRouteResult.RouteName);
                Assert.AreEqual(1, redirectToRouteResult.RouteValues.Count);
                using (var routeValuesEnumerator = redirectToRouteResult.RouteValues.GetEnumerator())
                {
                    routeValuesEnumerator.MoveNext();
                    Assert.AreEqual("action", routeValuesEnumerator.Current.Key);
                    Assert.AreEqual("Index", routeValuesEnumerator.Current.Value);
                }
            }

            [TestMethod]
            public void Get_Delete_Executed_With_Valid_Id_Setting_Mock_To_MembershipService_To_Return_A_User__Returns_ViewResult()
            {
                // Arrange
                var accountController = SutAs<AccountController>();
                var id = Guid.NewGuid();
                accountController.MembershipService = SetupMock<IMembershipService>().SetupToReturnARequestedUserModelById(id).Object;

                // Act
                var result = accountController.Delete(id);

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsNotNull(viewResult.Model);
                Assert.IsInstanceOfType(viewResult.Model, typeof(UserModel));
                var userModel = viewResult.Model as UserModel;
                Assert.AreEqual(id, userModel.Id);
            }

            // post Delete
            [TestMethod]
            public void Post_Delete_Executed_With_Null_Model__Returns_RedirectToRouteResult()
            {
                // Arrange

                // Act
                var result = SutAs<AccountController>().Delete(default(UserDetailModel));

                // Assert
                var redirectToRouteResult = AssertAndGetRedirectToRouteResult(result);
                Assert.AreEqual("", redirectToRouteResult.RouteName);
                Assert.AreEqual(1, redirectToRouteResult.RouteValues.Count);
                using (var routeValuesEnumerator = redirectToRouteResult.RouteValues.GetEnumerator())
                {
                    routeValuesEnumerator.MoveNext();
                    Assert.AreEqual("action", routeValuesEnumerator.Current.Key);
                    Assert.AreEqual("Index", routeValuesEnumerator.Current.Value);
                }
                Assert.IsNull(sut.TempData["DeleteSuccess"]);
            }

            [TestMethod]
            public void Post_Delete_Executed_With_Non_Null_Model_With_User_Id_Equals_Current_User_Id__Returns_RedirectToRouteResult_With_TempData_DeleteSuccess_False()
            {
                // Arrange
                var model = new UserDetailModel { Id = Guid.NewGuid() };
                SetHttpContextMockIntoSUT(model.Id);

                // Act
                var result = SutAs<AccountController>().Delete(model);

                // Assert
                var redirectToRouteResult = AssertAndGetRedirectToRouteResult(result);
                Assert.AreEqual("", redirectToRouteResult.RouteName);
                Assert.AreEqual(1, redirectToRouteResult.RouteValues.Count);
                using (var routeValuesEnumerator = redirectToRouteResult.RouteValues.GetEnumerator())
                {
                    routeValuesEnumerator.MoveNext();
                    Assert.AreEqual("action", routeValuesEnumerator.Current.Key);
                    Assert.AreEqual("Index", routeValuesEnumerator.Current.Value);
                }
                Assert.AreEqual(false, sut.TempData["DeleteSuccess"]);
            }

            [TestMethod]
            public void Post_Delete_Executed_With_Non_Null_Model_With_User_Id_Not_Equal_Current_User_Id_Without_Controller_Mocks__Throws_NullReferenceException()
            {
                // Arrange
                var model = new UserDetailModel { Id = Guid.NewGuid() };
                SetHttpContextMockIntoSUT(Guid.NewGuid());

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<AccountController>().Delete(model));
            }

            [TestMethod]
            public void Post_Delete_Executed_With_Non_Null_Model_With_User_Id_N1ot_Equal_Current_User_Id_With_Mocked_IMembershipService__Returns_TempData_DeleteSuccess_True()
            {
                // Arrange
                var model = new UserDetailModel { Id = Guid.NewGuid() };
                SetHttpContextMockIntoSUT(Guid.NewGuid());
                var accountController = SutAs<AccountController>();
                accountController.MembershipService = SetupMock<IMembershipService>().SetupToReturnARequestedUserModelById(model.Id)
                                                                                     .Object;
                // Act
                var result = accountController.Delete(model);

                var redirectToRouteResult = AssertAndGetRedirectToRouteResult(result);
                Assert.AreEqual("", redirectToRouteResult.RouteName);
                Assert.AreEqual(1, redirectToRouteResult.RouteValues.Count);
                using (var routeValuesEnumerator = redirectToRouteResult.RouteValues.GetEnumerator())
                {
                    routeValuesEnumerator.MoveNext();
                    Assert.AreEqual("action", routeValuesEnumerator.Current.Key);
                    Assert.AreEqual("Index", routeValuesEnumerator.Current.Value);
                }
                Assert.AreEqual(true, sut.TempData["DeleteSuccess"]);
            }

            // get Edit
            [TestMethod]
            public void Get_Edit_Executed_With_Null_Parameters__Throws_NullReferenceException()
            {
                // Arrange
                SetHttpContextMockIntoSUT(Guid.Empty);

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<AccountController>().Edit(default(UserEditModel)));
            }

            [TestMethod]
            public void Get_Edit_Executed_When_User_Is_Unknown_And_Resulting_Model_Is_Null__Stays_On_View()
            {
                // Arrange
                var userid = Guid.NewGuid();
                SetHttpContextMockIntoSUT(userid);
                SetupMembershipServiceMockIntoSUT();
                BindModelToController(userid);

                // act
                var result = SutAs<AccountController>().Edit(userid);

                // assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
                var viewResult = result as ViewResult;
                Assert.IsNotNull(viewResult);
                Assert.IsNull(viewResult.Model);
            }

            [TestMethod]
            public void Get_Edit_Executed_When_Non_Admin_User_Queries_Other_UserId__Gets_Redirected_To_Home_Unauthorized()
            {
                // Arrange
                var userid = Guid.NewGuid();
                SetHttpContextMockIntoSUT(userid);
                BindModelToController(userid);

                // act
                var result = SutAs<AccountController>().Edit(Guid.NewGuid());

                // assert
                AssertRedirectToHomeUnauthorized(result);
            }

            [TestMethod]
            public void Get_Edit_Executed_When_Admin_User_Queries_Other_User_Id_And_Resulting_Model_Is_Null__Stays_On_View()
            {
                // Arrange
                var userid = Guid.NewGuid();
                var otherId = Guid.NewGuid();
                SetHttpContextMockIntoSUT(userid);
                SetupMembershipServiceMockIntoSUT();
                SetupUserAsAdmin();
                BindModelToController(userid);

                // act
                var result = SutAs<AccountController>().Edit(otherId);
                var viewResult = result as ViewResult;

                // assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
                Assert.IsNotNull(viewResult);
                Assert.IsNull(viewResult.Model);
            }

            [TestMethod]
            public void Get_Edit_Executed_When_User_Is_Known_And_Resulting_Model_Is_Not_Null__Stays_On_View()
            {
                // Arrange
                var userId = Guid.NewGuid();
                SetHttpContextMockIntoSUT(userId);
                SetupMembershipServiceMockIntoSUT();
                BindModelToController(userId);
                membershipServiceMock.Setup(m => m.GetUserModel(userId))
                                     .Returns(new UserModel
                                     {
                                         Id = userId,
                                         Username = "Username",
                                         GivenName = "Given",
                                         Surname = "Sur",
                                         Email = "email"
                                     });

                var allRoles = new string[] { "role1", "role2" };
                var selectedRoles = new string[] { "role1" };
                SetupRolesProviderMockIntoSUT();
                roleProviderMock.Setup(r => r.GetAllRoles())
                                .Returns(allRoles);
                roleProviderMock.Setup(r => r.GetRolesForUser(userId))
                                .Returns(selectedRoles);

                // act
                var result = SutAs<AccountController>().Edit(userId);
                var viewResult = result as ViewResult;
                var userEditModel = viewResult?.Model as UserEditModel;

                // assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
                Assert.IsNotNull(viewResult);
                Assert.IsNotNull(viewResult.Model);
                Assert.IsInstanceOfType(viewResult.Model, typeof(UserEditModel));
                Assert.AreEqual(userEditModel.Id, userId);
                Assert.AreEqual(userEditModel.Username, "Username");
                Assert.AreEqual(userEditModel.Name, "Given");
                Assert.AreEqual(userEditModel.Surname, "Sur");
                Assert.AreEqual(userEditModel.Email, "email");
                Assert.AreEqual(userEditModel.Roles, allRoles);
                Assert.AreEqual(userEditModel.SelectedRoles, selectedRoles);
            }

            // post Edit
            [TestMethod]
            public void Post_Edit_Executed_With_Unbound_Empty_Model_And_Environment_Unprepared__Throws_A_NullReferenceException()
            {
                // Arrange
                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<AccountController>().Edit(new UserEditModel()));
            }

            [TestMethod]
            public void Post_Edit_Executed_With_Unbound_Bare_Model_Data_Referring_To_The_Same_User__Returns_Weird_Data()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var model = new UserEditModel { Id = userId };

                SetupMinimalEnvironment(userId);

                // act
                var result = SutAs<AccountController>().Edit(model);

                AssertMinimalViewResult(result, userId);
                AssertSuccessfulResponse(result);
            }

            [TestMethod]
            public void Post_Edit_Executed_With_Unbound_Bare_Model_And_Admin_And_DemoActive_Configuration_Set_And_Data_Referring_To_The_Same_UserId__Redirects_To_Home_Unauthorized()
            {
                // Arrange
                ConfigurationManager.AppSettings["demoModeActive"] = "true";
                // AuthenticationSettings class controller needs to run again because we changed the demoModelActive appSetting
                ReinitializeStaticClass(typeof(AuthenticationSettings));

                var userId = Guid.NewGuid();
                SetupMinimalEnvironment(userId);
                var model = new UserEditModel { Id = userId };
                SetupUserAsAdmin();

                // Act
                var result = SutAs<AccountController>().Edit(model);

                // Assert
                AssertRedirectToHomeUnauthorized(result);
            }

            [TestMethod]
            public void Post_Edit_Executed_With_Bound_Empty_Model_And_Correct_Environment__Passes_Execution()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();
                SetupMembershipServiceMockIntoSUT();
                SetupRolesProviderMockIntoSUT();
                var model = new UserEditModel();
                BindModelToController(model);

                // Act
                var result = SutAs<AccountController>().Edit(model);

                // Assert
                AssertMinimalViewResult(result, Guid.Empty);
            }

            [TestMethod]
            public void Post_EditExecuted__With_Bound_Bare_Model_Data_Referring_To_The_Same_User__Returns_Null_Update_Success_In_ViewBag()
            {
                // Arrange
                var userId = Guid.NewGuid();
                SetupMinimalEnvironment(userId);
                var model = new UserEditModel { Id = userId };
                BindModelToController(model);

                // act
                var result = SutAs<AccountController>().Edit(model);

                AssertMinimalViewResult(result, userId);
                Assert.IsFalse(sut.ModelState.IsValid);
                Assert.AreEqual(4, sut.ModelState.Count);
                Assert.IsNull((result as ViewResult).ViewBag.UpdateSuccess);
            }

            [TestMethod]
            public void Post_Edit_Executed_With_Bound_Bare_Model_Data_Referring_To_Another_User__Redirects_To_Home_Unauthorized()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var otherId = Guid.NewGuid();
                SetupMinimalEnvironment(userId);
                var model = new UserEditModel { Id = otherId };
                BindModelToController(model);

                // act
                var result = SutAs<AccountController>().Edit(model);

                // assert
                Assert.IsNotNull(result);

                AssertRedirectToHomeUnauthorized(result);
            }

            [TestMethod]
            public void Post_Edit_Executed_With_Bound_Invalid_Model_With_NewPassword_Not_Empty_And_OldPassword_Empty__Returns_Expected_ModelState()
            {
                // Arrange
                var userId = Guid.NewGuid();
                SetupMinimalEnvironment(userId);
                var model = new UserEditModel
                {
                    Id = userId,
                    Username = "Username",
                    Name = "Name",
                    Surname = "Surname",
                    Email = "email@test.com",
                    NewPassword = "NewPassword"
                };
                BindModelToController(model);

                // Act
                var result = SutAs<AccountController>().Edit(model);

                // Assert
                AssertMinimalViewResult(result, userId);
                Assert.IsFalse(sut.ModelState.IsValid);

                var expectedMessageForConfirmPassword = string.Format(Resources.Validation_Compare, "Confirm Password", "New Password");
                var actualMessageForConfirmPassword = sut.ModelState["ConfirmPassword"].Errors[0].ErrorMessage;

                Assert.AreEqual(expectedMessageForConfirmPassword, actualMessageForConfirmPassword);
            }

            [TestMethod]
            public void Post_Edit_Executed_With_Bound_Valid_Model_Data_Referring_To_The_Same_User__Returns_Data_From_The_User()
            {
                // Arrange
                var userId = Guid.NewGuid();
                SetupMinimalEnvironment(userId);

                var model = new UserEditModel
                {
                    Id = userId,
                    Username = "Username",
                    Name = "Name",
                    Surname = "Surname",
                    Email = "email@test.com",
                };
                BindModelToController(model);

                // act
                var result = SutAs<AccountController>().Edit(model);

                AssertMinimalViewResult(result, userId);
                AssertSuccessfulResponse(result);
            }

            // get Create
            [TestMethod]
            public void Get_Create_Executed_Without_Any_Setup__Throws_NullReferenceException()
            {
                // Arrange

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<AccountController>().Create());
            }

            [TestMethod]
            public void Get_Create_Executed_With_HttpContext_Setup__Throws_ArgumentNullException()
            {
                // Arrange
                ReinitializeStaticClass(typeof(ConfigurationEntry<UserConfiguration>));
                SetHttpContextMockIntoSUT(Guid.NewGuid());

                // Act & Assert
                Assert.ThrowsException<ArgumentNullException>(() => SutAs<AccountController>().Create());
            }

            [TestMethod]
            public void Get_Create_Executed_With_HttpContext_And_UserConfiguration_Setup__Returns_RedirectToRouteResult_To_Home_Unauthorized()
            {
                // Arrange
                SetHttpContextMockIntoSUT(Guid.NewGuid());
                ArrangeUserConfiguration();

                // Act
                var result = SutAs<AccountController>().Create();

                // Assert
                var redirectToRouteResult = AssertAndGetRedirectToRouteResult(result);
                Assert.AreEqual("", redirectToRouteResult.RouteName);
                Assert.AreEqual(2, redirectToRouteResult.RouteValues.Count);
                using (var routeValuesEnumerator = redirectToRouteResult.RouteValues.GetEnumerator())
                {
                    routeValuesEnumerator.MoveNext();
                    Assert.AreEqual("action", routeValuesEnumerator.Current.Key);
                    Assert.AreEqual("Unauthorized", routeValuesEnumerator.Current.Value);
                    routeValuesEnumerator.MoveNext();
                    Assert.AreEqual("controller", routeValuesEnumerator.Current.Key);
                    Assert.AreEqual("Home", routeValuesEnumerator.Current.Value);
                }
            }

            // post Create

            private void SetupMinimalEnvironment(Guid userId)
            {
                SetHttpContextMockIntoSUT(userId);
                SetupMembershipServiceMockIntoSUT();
                SetupRolesProviderMockIntoSUT();
            }

            private static void AssertMinimalViewResult(ActionResult result, Guid userId)
            {
                // assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));

                var viewResult = result as ViewResult;

                Assert.IsNotNull(viewResult);
                Assert.IsNotNull(viewResult.Model);
                Assert.IsInstanceOfType(viewResult.Model, typeof(UserEditModel));

                var userEditModel = viewResult.Model as UserEditModel;

                Assert.IsNotNull(userEditModel);
                Assert.AreEqual(userId, userEditModel.Id);
                Assert.IsNotNull(userEditModel.Roles);
                Assert.IsNotNull(viewResult.ViewBag);
            }

            private void AssertSuccessfulResponse(ActionResult result)
            {
                Assert.IsTrue(sut.ModelState.IsValid);

                var viewResult = result as ViewResult;

                Assert.IsTrue(viewResult.ViewBag.UpdateSuccess);
            }

            private void SetupMembershipServiceMockIntoSUT()
            {
                membershipServiceMock = new Mock<IMembershipService>();
                SutAs<AccountController>().MembershipService = membershipServiceMock.Object;
            }

            private void SetupRolesProviderMockIntoSUT()
            {
                roleProviderMock = new Mock<IRoleProvider>();
                SutAs<AccountController>().RoleProvider = roleProviderMock.Object;
            }

            private Mock<IMembershipService> membershipServiceMock;
            private Mock<IRoleProvider> roleProviderMock;
        }
    }
}