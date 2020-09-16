using Bonobo.Git.Server.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Test.Unit
{
    public partial class PRGPatternTests
    {
        [TestClass]
        public class AccountController_Post_Edit : PRGPatternTests
        {
            [TestMethod]
            public void Executed_With_Empty_Model_Throws_A_NullReferenceException()
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
            public void Executed_With_Minimal_Model_Data_Returs_Minimal_Result()
            {
                // Arrange
                Guid userId = Guid.Empty;
                SetupMinimalEnvironment(userId);
                UserEditModel model = new UserEditModel { Id = userId };

                // act
                var result = sut.Edit(model);

                // assert
                AssertMinimalViewResult(result, userId);
            }

            [TestMethod]
            public void Executed_With_Model_Data_Referring_To_The_Same_User_Returns_Data_From_The_User()
            {
                // Arrange
                Guid userId = Guid.NewGuid();
                SetupMinimalEnvironment(userId);
                UserEditModel model = new UserEditModel { Id = userId };

                // act
                var result = sut.Edit(model);

                AssertMinimalViewResult(result, userId);
            }

            [TestMethod]
            public void Executed_With_Model_Data_Referring_To_Other_User_Returns_Data_From_The_User()
            {
                // Arrange
                Guid userId = Guid.NewGuid();
                Guid otherId = Guid.NewGuid();
                SetupMinimalEnvironment(userId);
                UserEditModel model = new UserEditModel { Id = otherId };

                // act
                var result = sut.Edit(model);

                // assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));

                var redirectToRouteResult = result as RedirectToRouteResult;
                Assert.AreEqual("Home", redirectToRouteResult.RouteValues["controller"]);
                Assert.AreEqual("Unauthorized", redirectToRouteResult.RouteValues["action"]);
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
                Assert.AreEqual(true, viewResult.ViewBag.UpdateSuccess);
            }
        }
    }
}