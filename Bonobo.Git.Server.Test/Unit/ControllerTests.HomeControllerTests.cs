using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Test.Unit
{
    public partial class ControllerTests
    {
        [TestClass]
        public class HomeControllerTests : ControllerTests
        {
            [TestInitialize]
            public void TestInitialize()
            {
                sut = new HomeController();
            }

            [TestMethod]
            public void Get_LogOn_Called_With_Null_Url__Throws_No_Exceptions()
            {
                // Arrange
                string returnUrl = null;

                // Act
                var result = SutAs<HomeController>().LogOn(returnUrl);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));

                var viewResult = result as ViewResult;

                Assert.IsNotNull(viewResult.Model);
                Assert.IsInstanceOfType(viewResult.Model, typeof(LogOnModel));

                var logOnModel = viewResult.Model as LogOnModel;
                Assert.IsNull(logOnModel.ReturnUrl);
            }

            [TestMethod]
            public void Get_LogOn_Called_With_Non_Null_Url__Returns_ReturnUrl_In_Model()
            {
                // Arrange
                const string returnUrl = "theurl";

                // Act
                var result = SutAs<HomeController>().LogOn(returnUrl);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));

                var viewResult = result as ViewResult;

                Assert.IsNotNull(viewResult.Model);
                Assert.IsInstanceOfType(viewResult.Model, typeof(LogOnModel));

                var logOnModel = viewResult.Model as LogOnModel;
                Assert.AreEqual(returnUrl, logOnModel.ReturnUrl);
            }

            [TestMethod]
            public void Post_Logon_Called_With_Null_Model__Throws_NullReferenceException()
            {
                // Arrange
                LogOnModel model = null;

                try
                {
                    // Act
                    SutAs<HomeController>().LogOn(model);
                }
                catch (NullReferenceException)
                {
                    return;
                }

                // Assert
                Assert.Fail();
            }

            [TestMethod]
            public void Post_Logon_Called_With_Empty_Model_And_Controller_Environment__Returns_ViewResult()
            {
                // Arrange
                ArrangeBareContext();

                membershipServiceMock.Setup(mp => mp.ValidateUser(It.IsAny<string>(), It.IsAny<string>()))
                                     .Returns(ValidationResult.Failure);
                LogOnModel model = new LogOnModel();

                // Act
                var result = SutAs<HomeController>().LogOn(model);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
            }

            [TestMethod]
            public void Post_Logon_Called_With_Bare_Model_Request_And_Good_Credentials__Returns_EmptyResult()
            {
                // Arrange
                ArrangeBareContext();

                const string username = "username";
                const string password = "password";
                membershipServiceMock.Setup(mp => mp.ValidateUser(username, password))
                                     .Returns(ValidationResult.Success);
                httpContextMock.SetupGet(hc => hc.Request)
                               .Returns(new Mock<HttpRequestBase>().Object);
                LogOnModel model = new LogOnModel { Username = username, Password = password };

                // Act
                var result = SutAs<HomeController>().LogOn(model);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(EmptyResult));
            }

            [TestMethod]
            public void Post_Logon_Called_With_Bare_Model_And_No_Authorization__Returns_RedirectResult()
            {
                // Arrange
                ArrangeBareContext();

                const string username = "username";
                const string password = "password";
                membershipServiceMock.Setup(mp => mp.ValidateUser(username, password))
                                     .Returns(ValidationResult.NotAuthorized);
                httpContextMock.SetupGet(hc => hc.Request)
                               .Returns(new Mock<HttpRequestBase>().Object);
                LogOnModel model = new LogOnModel { Username = username, Password = password };

                // Act
                var result = SutAs<HomeController>().LogOn(model);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(RedirectResult));

                var redirectResult = result as RedirectResult;
                Assert.AreEqual("~/Home/Unauthorized", redirectResult.Url);
            }

            [TestMethod]
            public void Post_Logon_Called_With_Bare_Model_And_Bad_Credentials__Returns_ViewResult()
            {
                // Arrange
                ArrangeBareContext();

                const string username = "username";
                const string password = "password";
                membershipServiceMock.Setup(mp => mp.ValidateUser(username, password))
                                     .Returns(ValidationResult.Failure);
                httpContextMock.SetupGet(hc => hc.Request)
                               .Returns(new Mock<HttpRequestBase>().Object);
                LogOnModel model = new LogOnModel { Username = username, Password = password };

                // Act
                var result = SutAs<HomeController>().LogOn(model);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
                Assert.IsFalse(sut.ModelState.IsValid);
                Assert.AreEqual(Resources.Home_LogOn_UsernamePasswordIncorrect, sut.ModelState[""].Errors[0].ErrorMessage);
            }

            // get ResetPassword
            // post ResetPassword
            // get ForgotPassword
            // post ForgotPassword

            private void ArrangeBareContext()
            {
                sut.ControllerContext = CreateControllerContext();
                SetupMembershipServiceMockIntoSUT();
                SutAs<HomeController>().AuthenticationProvider = new Mock<IAuthenticationProvider>().Object;
                SutAs<HomeController>().Url = new Mock<UrlHelper>().Object;
            }

            private void SetupMembershipServiceMockIntoSUT()
            {
                membershipServiceMock = new Mock<IMembershipService>();
                SutAs<HomeController>().MembershipService = membershipServiceMock.Object;
            }

            private Mock<IMembershipService> membershipServiceMock;
        }
    }
}
