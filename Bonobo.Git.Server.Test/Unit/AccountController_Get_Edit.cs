using Bonobo.Git.Server.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Test.Unit
{
    public partial class PRGPatternTests
    {
        [TestClass]
        public class AccountController_Get_Edit : PRGPatternTests
        {
            [TestMethod]
            public void Cannot_Handle_Null_Parameters()
            {
                try
                {
                    // arrange
                    SetHttpContextMockIntoSUT(Guid.Empty);
                    // act
                    var result = sut.Edit(null);
                }
                catch (NullReferenceException)
                {
                    return;
                }
                //assert
                Assert.Fail();
            }

            [TestMethod]
            public void Stays_On_View_When_User_Is_Unknown_And_Resulting_Model_Is_Null()
            {
                // Arrange
                Guid userid = Guid.NewGuid();
                SetHttpContextMockIntoSUT(userid);
                SetupMembershipServiceMockIntoSUT();

                // act
                var result = sut.Edit(userid);

                // assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
                var viewResult = result as ViewResult;
                Assert.IsNotNull(viewResult);
                Assert.IsNull(viewResult.Model);
            }

            [TestMethod]
            public void Gets_Redirected_To_Home_Unauthorized_When_NonAdmin_User_Queries_Other_User_Id()
            {
                // Arrange
                Guid userid = Guid.NewGuid();
                SetHttpContextMockIntoSUT(userid);

                // act
                var result = sut.Edit(Guid.NewGuid());

                // assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
                var redirectToRouteResult = result as RedirectToRouteResult;
                Assert.IsNotNull(redirectToRouteResult);
                Assert.AreEqual("Home", redirectToRouteResult.RouteValues["controller"]);
                Assert.AreEqual("Unauthorized", redirectToRouteResult.RouteValues["action"]);
            }

            [TestMethod]
            public void Stays_On_View_When_Admin_User_Queries_Other_User_Id_And_Resulting_Model_Is_Null()
            {
                // Arrange
                Guid userid = Guid.NewGuid();
                Guid otherId = Guid.NewGuid();
                SetHttpContextMockIntoSUT(userid);
                SetupMembershipServiceMockIntoSUT();
                SetupUserAsAdmin();

                // act
                var result = sut.Edit(otherId);
                var viewResult = result as ViewResult;

                // assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
                Assert.IsNotNull(viewResult);
                Assert.IsNull(viewResult.Model);
            }

            [TestMethod]
            public void Stays_On_View_When_User_Is_Known_And_Resulting_Model_Is_Not_Null()
            {
                // Arrange
                Guid id = Guid.NewGuid();
                SetHttpContextMockIntoSUT(id);
                SetupMembershipServiceMockIntoSUT();
                membershipServiceMock.Setup(m => m.GetUserModel(id))
                                     .Returns(new UserModel
                                     {
                                         Id = id,
                                         Username = "Username",
                                         GivenName = "Given",
                                         Surname = "Sur",
                                         Email = "email"
                                     });

                string[] allRoles = new string[] { "role1", "role2" };
                string[] selectedRoles = new string[] { "role1" };
                SetupRolesProviderMockIntoSUT();
                roleProviderMock.Setup(r => r.GetAllRoles())
                                .Returns(allRoles);
                roleProviderMock.Setup(r => r.GetRolesForUser(id))
                                .Returns(selectedRoles);

                // act
                var result = sut.Edit(id);
                var viewResult = result as ViewResult;
                var userEditModel = viewResult?.Model as UserEditModel;

                // assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
                Assert.IsNotNull(viewResult);
                Assert.IsNotNull(viewResult.Model);
                Assert.IsInstanceOfType(viewResult.Model, typeof(UserEditModel));
                Assert.AreEqual(userEditModel.Id, id);
                Assert.AreEqual(userEditModel.Username, "Username");
                Assert.AreEqual(userEditModel.Name, "Given");
                Assert.AreEqual(userEditModel.Surname, "Sur");
                Assert.AreEqual(userEditModel.Email, "email");
                Assert.AreEqual(userEditModel.Roles, allRoles);
                Assert.AreEqual(userEditModel.SelectedRoles, selectedRoles);
            }
        }
    }
}