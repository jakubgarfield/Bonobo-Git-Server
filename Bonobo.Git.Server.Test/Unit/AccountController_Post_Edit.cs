using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Configuration;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Test.Unit
{
    public partial class PRGPatternTests
    {
        [TestClass]
        public class AccountController_Post_Edit : PRGPatternTests
        {
            [TestMethod]
            public void Executed_With_Unbound_Empty_Model_And_Environment_Unprepared__Throws_A_NullReferenceException()
            {
                try
                {
                    sut.Edit(new UserEditModel());
                }
                catch (NullReferenceException)
                {
                    return;
                }
                Assert.Fail();
            }

            [TestMethod]
            public void Executed_With_Unbound_Bare_Model_Data_Referring_To_The_Same_User__Returns_Weird_Data()
            {
                // Arrange
                Guid userId = Guid.NewGuid();
                UserEditModel model = new UserEditModel { Id = userId };

                SetupMinimalEnvironment(userId);
                SetupCookiesCollectionToHttpResponse();

                // act
                var result = sut.Edit(model);

                AssertMinimalViewResult(result, userId);
                AssertSuccessfulResponse(result);
            }

            [TestMethod]
            public void Executed_With_Unbound_Bare_Model_And_Admin_And_DemoActive_Configuration_Set_And_Data_Referring_To_The_Same_UserId__Redirects_To_Home_Unauthorized()
            {
                // Arrange
                ConfigurationManager.AppSettings["demoModeActive"] = "true";
                ReinitializeAuthenticationSettingsStaticClass();

                Guid userId = Guid.NewGuid();
                SetupMinimalEnvironment(userId);
                UserEditModel model = new UserEditModel { Id = userId };
                SetupUserAsAdmin();

                // Act
                var result = sut.Edit(model);

                // Assert
                AssertRedirectToHomeUnauthorized(result);
            }

            [TestMethod]
            public void Executed_With_Bound_Empty_Model_And_Correct_Environment__Passes_Execution()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContextFromPrincipal(new Mock<IPrincipal>().Object);
                SetupMembershipServiceMockIntoSUT();
                SetupRolesProviderMockIntoSUT();
                UserEditModel model = new UserEditModel();
                BindModelToController(model);

                // Act
                var result = sut.Edit(model);

                // Assert
                AssertMinimalViewResult(result, Guid.Empty);
            }

            [TestMethod]
            public void Executed_With_Bound_Bare_Model_Data_Referring_To_The_Same_User__Returns_Null_Update_Success_In_ViewBag()
            {
                // Arrange
                Guid userId = Guid.NewGuid();
                SetupMinimalEnvironment(userId);
                UserEditModel model = new UserEditModel { Id = userId };
                BindModelToController(model);

                // act
                var result = sut.Edit(model);

                AssertMinimalViewResult(result, userId);
                Assert.IsFalse(sut.ModelState.IsValid);
                Assert.AreEqual(4, sut.ModelState.Count);
                Assert.IsNull((result as ViewResult).ViewBag.UpdateSuccess);
            }

            [TestMethod]
            public void Executed_With_Bound_Bare_Model_Data_Referring_To_Another_User__Redirects_To_Home_Unauthorized()
            {
                // Arrange
                Guid userId = Guid.NewGuid();
                Guid otherId = Guid.NewGuid();
                SetupMinimalEnvironment(userId);
                UserEditModel model = new UserEditModel { Id = otherId };
                BindModelToController(model);

                // act
                var result = sut.Edit(model);

                // assert
                Assert.IsNotNull(result);

                AssertRedirectToHomeUnauthorized(result);
            }

            [TestMethod]
            public void Executed_With_Bound_Invalid_Model_With_NewPassword_Not_Empty_And_OldPassword_Empty__Returns_Expected_ModelState()
            {
                // Arrange
                Guid userId = Guid.NewGuid();
                SetupMinimalEnvironment(userId);
                UserEditModel model = new UserEditModel
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
                var result = sut.Edit(model);

                // Assert
                AssertMinimalViewResult(result, userId);
                Assert.IsFalse(sut.ModelState.IsValid);

                string expectedMessageForConfirmPassword = String.Format(Resources.Validation_Compare, "Confirm Password", "New Password");
                string actualMessageForConfirmPassword = sut.ModelState["ConfirmPassword"].Errors[0].ErrorMessage;

                Assert.AreEqual(expectedMessageForConfirmPassword, actualMessageForConfirmPassword);
            }

            [TestMethod]
            public void Executed_With_Bound_Valid_Model_Data_Referring_To_The_Same_User__Returns_Data_From_The_User()
            {
                // Arrange
                Guid userId = Guid.NewGuid();
                SetupMinimalEnvironment(userId);
                SetupCookiesCollectionToHttpResponse();

                UserEditModel model = new UserEditModel
                {
                    Id = userId,
                    Username = "Username",
                    Name = "Name",
                    Surname = "Surname",
                    Email = "email@test.com",
                };
                BindModelToController(model);

                // act
                var result = sut.Edit(model);

                AssertMinimalViewResult(result, userId);
                AssertSuccessfulResponse(result);
            }

            private static void ReinitializeAuthenticationSettingsStaticClass()
            {
                // This code allows reinitializing AuthenticationSettings static class after changing a configuration app setting
                // see: https://stackoverflow.com/a/51758748/41236
                //
                typeof(AuthenticationSettings).TypeInitializer.Invoke(null, null);
            }

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

                ViewResult viewResult = result as ViewResult;

                Assert.IsTrue(viewResult.ViewBag.UpdateSuccess);
            }

            private void SetupCookiesCollectionToHttpResponse()
            {
                var responseMock = new Mock<HttpResponseBase>();

                HttpCookieCollection cookies = new HttpCookieCollection();
                responseMock.SetupGet(r => r.Cookies)
                            .Returns(cookies);
                httpContextMock.SetupGet(c => c.Response)
                               .Returns(responseMock.Object);
            }
        }
    }
}