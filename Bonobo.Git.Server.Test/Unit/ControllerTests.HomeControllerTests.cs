using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Helpers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Text;
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

            // get Index
            [TestMethod]
            public void Get_Index__Returns_RedirectToRouteResult_To_Repository_Index_With_Zero_ViewData()
            {
                // arrange

                // act
                var result = SutAs<HomeController>().Index();

                // assert
                var redirectToRouteResult = AssertAndGetRedirectToRouteResult(result);
                Assert.AreEqual(0, sut.ViewData.Count);
                Assert.AreEqual(2, redirectToRouteResult.RouteValues.Count);
                var enumerator = redirectToRouteResult.RouteValues.GetEnumerator();
                enumerator.MoveNext();
                Assert.AreEqual("action", enumerator.Current.Key);
                Assert.AreEqual("Index", enumerator.Current.Value);
                enumerator.MoveNext();
                Assert.AreEqual("controller", enumerator.Current.Key);
                Assert.AreEqual("Repository", enumerator.Current.Value);
                enumerator.Dispose();
            }

            // get PageNotFound
            [TestMethod]
            public void Get_PageNotFound__Returns_ViewResult()
            {
                // arrange

                // act
                var result = SutAs<HomeController>().PageNotFound();

                // assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.AreEqual("", viewResult.ViewName);
            }

            // get ServerError
            [TestMethod]
            public void Get_ServerError__Returns_ViewResult()
            {
                // arrange

                // act
                var result = SutAs<HomeController>().ServerError();

                // assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.AreEqual("", viewResult.ViewName);
            }

            // get Error
            [TestMethod]
            public void Get_Error__Returns_ViewResult()
            {
                // arrange

                // act
                var result = SutAs<HomeController>().Error();

                // assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.AreEqual("", viewResult.ViewName);
            }

            // get ResetPassword
            [TestMethod]
            public void Get_ResetPassword_Called_With_Invalid_Digest__Returns_RedirectToRouteResult_To_Home_Index_With_Invalid_ModelState_And_An_Error()
            {
                // arrange

                // act
                var result = SutAs<HomeController>().ResetPassword(string.Empty);

                // assert
                var redirectToRouteResult = AssertAndGetRedirectToRouteResult(result);
                Assert.AreEqual(2, redirectToRouteResult.RouteValues.Count);
                var routeValuesEnumerator = redirectToRouteResult.RouteValues.GetEnumerator();
                try
                {
                    routeValuesEnumerator.MoveNext();
                    Assert.AreEqual("action", routeValuesEnumerator.Current.Key);
                    Assert.AreEqual("Index", routeValuesEnumerator.Current.Value);
                    routeValuesEnumerator.MoveNext();
                    Assert.AreEqual("controller", routeValuesEnumerator.Current.Key);
                    Assert.AreEqual("Home", routeValuesEnumerator.Current.Value);
                }
                finally
                {
                    routeValuesEnumerator.Dispose();
                }
                Assert.IsFalse(sut.ModelState.IsValid);
                Assert.AreEqual(1, sut.ModelState.Count);
                var modelStateEnumerator = sut.ModelState.GetEnumerator();
                try
                {
                    modelStateEnumerator.MoveNext();
                    Assert.AreEqual("", modelStateEnumerator.Current.Key);
                    Assert.AreEqual(1, modelStateEnumerator.Current.Value.Errors.Count);
                    var modelStateErrorsEnumerator = modelStateEnumerator.Current.Value.Errors.GetEnumerator();
                    try
                    {
                        AssertNextErrorMessageIs(modelStateErrorsEnumerator, "Password reset link was not valid");
                    }
                    finally
                    {
                        modelStateErrorsEnumerator.Dispose();
                    }
                }
                finally
                {
                    modelStateEnumerator.Dispose();
                }
            }

            //
            // IGNORDED TEST!
            //
            // Can't continue developing this particular test because there's a hard dependency on BonoboGitServerContext that I won't break right now
            //
            // NOTE: Most of the code of this test comes from the test with an invalid digest, it's going to evolve when the hard dependency is removed
            //
            // IGNORDED TEST!
            // 
            [TestMethod, Ignore]
            public void Get_ResetPassword_Called_With_Valid_Digest__THIS_TEST_IS_IGNORED()
            {
                // arrange
                var homeController = SutAs<HomeController>();
                PrepairCache("valid", "username");

                // act
                var result = homeController.ResetPassword("valid");

                // assert
                var redirectToRouteResult = AssertAndGetRedirectToRouteResult(result);
                Assert.AreEqual(2, redirectToRouteResult.RouteValues.Count);
                var routeValuesEnumerator = redirectToRouteResult.RouteValues.GetEnumerator();
                try
                {
                    routeValuesEnumerator.MoveNext();
                    Assert.AreEqual("action", routeValuesEnumerator.Current.Key);
                    Assert.AreEqual("Index", routeValuesEnumerator.Current.Value);
                    routeValuesEnumerator.MoveNext();
                    Assert.AreEqual("controller", routeValuesEnumerator.Current.Key);
                    Assert.AreEqual("Home", routeValuesEnumerator.Current.Value);
                }
                finally
                {
                    routeValuesEnumerator.Dispose();
                }
                Assert.IsFalse(sut.ModelState.IsValid);
                Assert.AreEqual(1, sut.ModelState.Count);
                var modelStateEnumerator = sut.ModelState.GetEnumerator();
                try
                {
                    modelStateEnumerator.MoveNext();
                    Assert.AreEqual("", modelStateEnumerator.Current.Key);
                    Assert.AreEqual(1, modelStateEnumerator.Current.Value.Errors.Count);
                    var modelStateErrorsEnumerator = modelStateEnumerator.Current.Value.Errors.GetEnumerator();
                    try
                    {
                        modelStateErrorsEnumerator.MoveNext();
                        Assert.AreEqual("Password reset link was not valid", modelStateErrorsEnumerator.Current.ErrorMessage);
                    }
                    finally
                    {
                        modelStateErrorsEnumerator.Dispose();
                    }
                }
                finally
                {
                    modelStateEnumerator.Dispose();
                }
            }

            // post ResetPassword
            [TestMethod]
            public void Post_ResetPassword_Called_With_Null_Parameter__Throws_NullReferenceException()
            {
                // Arrange
                try
                {
                    // Act
                    SutAs<HomeController>().ResetPassword(default(ResetPasswordModel));
                }
                catch (NullReferenceException)
                {
                    return;
                }
                // Assert
                Assert.Fail();
            }

            [TestMethod]
            public void Post_ResetPassword_Called_With_Empty_And_No_Binding_Model__Throws_UnauthorizedAccessException()
            {
                // Arrange
                var model = new ResetPasswordModel();

                // Act
                try
                {
                    SutAs<HomeController>().ResetPassword(model);
                }
                // Assert
                catch (ArgumentNullException ane)
                {
                    Assert.AreEqual("Value cannot be null.\r\nParameter name: key", ane.Message);
                    return;
                }
                Assert.Fail();
            }

            [TestMethod]
            public void Post_ResetPassword_Called_With_Model_With_Empty_Digest_And_No_Binding_Model__Throws_UnauthorizedAccessException()
            {
                // Arrange
                var model = new ResetPasswordModel { Digest = string.Empty };

                // Act
                try
                {
                    SutAs<HomeController>().ResetPassword(model);
                }
                // Assert
                catch (UnauthorizedAccessException uae)
                {
                    Assert.AreEqual("Invalid password reset form", uae.Message);
                    return;
                }
                Assert.Fail();
            }

            [TestMethod]
            public void Post_ResetPassword_Called_With_Empty_And_Binding_Model__Returns_ViewResult_With_Invalid_ModelState()
            {
                // Arrange
                var model = new ResetPasswordModel();
                BindModelToController(model);

                // Act
                var result = SutAs<HomeController>().ResetPassword(model);

                // Assert
                Assert.IsFalse(sut.ModelState.IsValid);
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsNotNull(viewResult.Model);
                Assert.AreEqual(3, sut.ModelState.Count);
                var modelStateEnumerator = sut.ModelState.GetEnumerator();
                try
                {
                    modelStateEnumerator.MoveNext();
                    Assert.AreEqual("Username", modelStateEnumerator.Current.Key);
                    var modelStateErrorEnumerator = modelStateEnumerator.Current.Value.Errors.GetEnumerator();
                    try
                    {
                        AssertNextErrorMessageIs(modelStateErrorEnumerator, "\"Username\" is mandatory.");
                    }
                    finally
                    {
                        modelStateErrorEnumerator.Dispose();
                    }
                    modelStateEnumerator.MoveNext();
                    Assert.AreEqual("Password", modelStateEnumerator.Current.Key);
                    modelStateErrorEnumerator = modelStateEnumerator.Current.Value.Errors.GetEnumerator();
                    try
                    {
                        AssertNextErrorMessageIs(modelStateErrorEnumerator, "\"Password\" is mandatory.");
                    }
                    finally
                    {
                        modelStateErrorEnumerator.Dispose();
                    }
                    modelStateEnumerator.MoveNext();
                    Assert.AreEqual("ConfirmPassword", modelStateEnumerator.Current.Key);
                    modelStateErrorEnumerator = modelStateEnumerator.Current.Value.Errors.GetEnumerator();
                    try
                    {
                        AssertNextErrorMessageIs(modelStateErrorEnumerator, "\"Confirm Password\" is mandatory.");
                    }
                    finally
                    {
                        modelStateErrorEnumerator.Dispose();
                    }
                }
                finally
                {
                    modelStateEnumerator.Dispose();
                }
            }

            [TestMethod]
            public void Post_ResetPassword_Called_With_Complete_Model_But_Invalid_And_Binding_Model__Returns_ViewResult_With_Invalid_ModelState()
            {
                // Arrange
                var model = new ResetPasswordModel
                {
                    Username = "Username",
                    Password = "Password",
                    ConfirmPassword = "ConfirmPassword",
                    Digest = "Digest"
                };
                BindModelToController(model);

                // Act
                var result = SutAs<HomeController>().ResetPassword(model);

                // Assert
                Assert.IsFalse(sut.ModelState.IsValid);
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsNotNull(viewResult.Model);
                Assert.AreEqual(1, sut.ModelState.Count);
                var modelStateEnumerator = sut.ModelState.GetEnumerator();
                try
                {
                    modelStateEnumerator.MoveNext();
                    Assert.AreEqual("ConfirmPassword", modelStateEnumerator.Current.Key);
                    var modelStateErrorEnumerator = modelStateEnumerator.Current.Value.Errors.GetEnumerator();
                    try
                    {
                        AssertNextErrorMessageIs(modelStateErrorEnumerator, "\"Confirm Password\" and \"Password\" must have the same value.");
                    }
                    finally
                    {
                        modelStateErrorEnumerator.Dispose();
                    }
                }
                finally
                {
                    modelStateEnumerator.Dispose();
                }
            }

            [TestMethod]
            public void Post_ResetPassword_Called_With_Complete_And_Valid_Model_And_Binding_Model_Not_Prepairing_Cache__Throws_UnauthorizedAccessException()
            {
                // Arrange
                var model = new ResetPasswordModel
                {
                    Username = "Username",
                    Password = "Password",
                    ConfirmPassword = "Password",
                    Digest = "Digest"
                };

                BindModelToController(model);

                // Act
                try
                {
                    SutAs<HomeController>().ResetPassword(model);
                }
                // Assert
                catch (UnauthorizedAccessException)
                {
                    return;
                }

                Assert.Fail();
            }

            //
            // IGNORDED TEST!
            //
            // Can't continue developing this particular test because there's a hard dependency on BonoboGitServerContext that I won't break right now
            //
            // NOTE: this test is going to evolve after the hard dependency is removed
            //
            // IGNORDED TEST!
            // 
            [TestMethod, Ignore]
            public void Post_ResetPassword_Called_With_Complete_And_Valid_Model_And_Binding_Model_Prepairing_Cache__Returns__THIS_TEST_IS_IGNORED()
            {
                // Arrange
                const string username = "Username";
                const string token = "Digest";

                var model = new ResetPasswordModel
                {
                    Username = username,
                    Password = "Password",
                    ConfirmPassword = "Password",
                    Digest = token
                };
                PrepairCache(token, username);
                BindModelToController(model);

                // Act
                var result = SutAs<HomeController>().ResetPassword(model);

                // assert
                Assert.Fail();
            }

            // get ForgotPassword
            [TestMethod]
            public void Get_ForgotPassword__Returns_ViewResult_With_Empty_Model()
            {
                // Arrange

                // Act
                var result = SutAs<HomeController>().ForgotPassword();

                // Assert
                Assert.IsTrue(sut.ModelState.IsValid);
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsNotNull(viewResult.Model);
                Assert.IsInstanceOfType(viewResult.Model, typeof(ForgotPasswordModel));
                var forgotPasswordModel = viewResult.Model as ForgotPasswordModel;
                Assert.IsNull(forgotPasswordModel.Username);
            }

            // post ForgotPassword
            [TestMethod]
            public void Post_ForgotPassword_Called_With_Null_Model_Without_Binding__Throws_NullReferenceException()
            {
                // Arrange
                var model = default(ForgotPasswordModel);

                try
                {
                    // Act
                    var result = SutAs<HomeController>().ForgotPassword(model);
                }
                // Assert
                catch (NullReferenceException)
                {
                    return;
                }
                Assert.Fail();
            }

            [TestMethod]
            public void Post_ForgotPassword_Called_With_Null_Model_And_Binding_It__Returns_ViewResult_With_Null_Model_And_Invalid_ModelState()
            {
                // Arrange
                var model = default(ForgotPasswordModel);
                BindModelToController(model);

                // Act
                var result = SutAs<HomeController>().ForgotPassword(model);

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsFalse(sut.ModelState.IsValid);
                Assert.IsNull(viewResult.Model);
            }

            [TestMethod]
            public void Post_ForgotPassword_Called_With_Empty_Model_Bound_To_Controller__Returns_ViewResult_With_Non_Null_Model_And_Invalid_ModelState()
            {
                // Arrange
                var model = new ForgotPasswordModel();
                BindModelToController(model);

                // Act
                var result = SutAs<HomeController>().ForgotPassword(model);

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsFalse(sut.ModelState.IsValid);
                Assert.IsNotNull(viewResult.Model);
                Assert.AreEqual(1, sut.ModelState.Count);
                var modelStateEnumerator = sut.ModelState.GetEnumerator();
                modelStateEnumerator.MoveNext();
                try
                {
                    Assert.AreEqual("Username", modelStateEnumerator.Current.Key);
                    Assert.AreEqual(1, modelStateEnumerator.Current.Value.Errors.Count);
                    var modelStateErrorEnumerator = modelStateEnumerator.Current.Value.Errors.GetEnumerator();
                    try
                    {
                        modelStateErrorEnumerator.MoveNext();
                        Assert.AreEqual("\"Username\" is mandatory.", modelStateErrorEnumerator.Current.ErrorMessage);
                    }
                    finally
                    {
                        modelStateErrorEnumerator.Dispose();
                    }
                }
                finally
                {
                    modelStateEnumerator.Dispose();
                }
            }

            [TestMethod]
            public void Post_ForgotPassword_Called_With_Valid_Model_Bound_To_Controller_With_Membership_Returning_Null__Returns_ViewResult_With_Non_Null_Model_And_Invalid_ModelState()
            {
                // Arrange
                var model = new ForgotPasswordModel { Username = "username" };
                BindModelToController(model);
                sut.ControllerContext = CreateControllerContext();
                var homeController = SutAs<HomeController>();
                homeController.MembershipService = SetupMock<IMembershipService>().Object;

                // Act
                var result = homeController.ForgotPassword(model);

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsFalse(sut.ModelState.IsValid);
                Assert.IsNotNull(viewResult.Model);
                Assert.AreEqual(1, sut.ModelState.Count);
                var modelStateEnumerator = sut.ModelState.GetEnumerator();
                try
                {
                    modelStateEnumerator.MoveNext();
                    Assert.AreEqual("", modelStateEnumerator.Current.Key);
                    Assert.AreEqual(1, modelStateEnumerator.Current.Value.Errors.Count);
                    var modelStateErrorEnumerator = modelStateEnumerator.Current.Value.Errors.GetEnumerator();
                    try
                    {
                        modelStateErrorEnumerator.MoveNext();
                        Assert.AreEqual("Could not find username", modelStateErrorEnumerator.Current.ErrorMessage);
                    }
                    finally
                    {
                        modelStateErrorEnumerator.Dispose();
                    }
                }
                finally
                {
                    modelStateEnumerator.Dispose();
                }
            }

            [TestMethod]
            public void Post_ForgotPassword_Called_With_Valid_Model_Bound_To_Controller_With_Membership_Returning_OK__Returns_ViewResult_With_Non_Null_Model_And_Valid_ModelState()
            {
                // Arrange
                var resetToken = "resetToken";
                var model = new ForgotPasswordModel { Username = "username" };
                BindModelToController(model);
                sut.ControllerContext = CreateControllerContext();
                sut.Url = new Mock<UrlHelper>().Object;
                var url = new Uri("http://localhost");
                requestMock.SetupGet(m => m.Url)
                           .Returns(url);
                var homeController = SutAs<HomeController>();
                homeController.MembershipHelper = SetupMock<MembershipHelper>().SetupToRespondTrueWhenSendingForgotPasswordEmail()
                                                                               .Object;
                homeController.MembershipService = SetupMock<IMembershipService>().SetupToReturnARequestedUserModel(model.Username)
                                                                                  .SetupToGenerateResetToken(resetToken, model.Username)
                                                                                  .Object;

                // Act
                var result = homeController.ForgotPassword(model);

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.IsTrue(sut.ModelState.IsValid);
                Assert.IsNotNull(viewResult.Model);
                Assert.AreEqual(true, sut.TempData["SendSuccess"]);
            }

            // get WindowsLogin
            [TestMethod]
            public void Get_WindowsLogin_Called_With_Defaut_Parameter_With_No_Arrange__Throws_NullReferenceException()
            {
                // Arrange
                try
                {
                    // Act
                    SutAs<HomeController>().WindowsLogin(default(string));
                }
                // Assert
                catch (NullReferenceException)
                {
                    return;
                }
                Assert.Fail();
            }

            [TestMethod]
            public void Get_WindowsLogin_Called_With_Defaut_Parameter_With_Arrange__Throws_ArgumentException()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext("user");

                try
                {
                    // Act
                    SutAs<HomeController>().WindowsLogin(default(string));
                }
                catch (ArgumentException)
                {
                    return;
                }

                // Assert
                Assert.Fail();
            }

            [TestMethod]
            public void Get_WindowsLogin_Called_With_Non_Or_Empty_Defaut_Parameter_With_Arrange__Returns_RedirectResult()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext("user");

                var result = SutAs<HomeController>().WindowsLogin("returnUrl");

                // Assert
                var redirectResult = AssertAndGetRedirectResult(result);
                Assert.AreEqual("returnUrl", redirectResult.Url);
            }

            [TestMethod]
            public void Get_WindowsLogin_Called_With_Non_Or_Empty_Defaut_Parameter_With_No_Identity_Arrange_And_Owin_Environment__Returns_EmptyResult()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext(string.Empty);
                SetupOwinEnvironment();
                var result = SutAs<HomeController>().WindowsLogin("returnUrl");

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(EmptyResult));
            }

            // get LogOn
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

            // get LogOnWithResetOption
            [TestMethod]
            public void Get_LogOnWithResetOption_Called_With_Default_Parameter__Returns_ViewModel_With_DatabaseResetCode_Minus_One()
            {
                // Arrange
                var returnUrl = default(string);

                // Act
                var result = SutAs<HomeController>().LogOnWithResetOption(returnUrl);

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                var expected = new LogOnModel { DatabaseResetCode = -1 };
                var actual = viewResult.Model as LogOnModel;
                Assert.AreEqual("LogOn", viewResult.ViewName);
                Assert.IsTrue(ArePropertiesEqual(expected, actual), $"\nExpected: <{expected}>.\nActual: <{actual}>.");
            }

            [TestMethod]
            public void Get_LogOnWithResetOption_Called_With_Non_Default_Parameter__Returns_ViewModel_With_DatabaseResetCode_Minus_One_And_ReturnUrl()
            {
                // Arrange
                var returnUrl = "returnUrl";

                // Act
                var result = SutAs<HomeController>().LogOnWithResetOption(returnUrl);

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                var expected = new LogOnModel { ReturnUrl = returnUrl, DatabaseResetCode = -1 };
                var actual = viewResult.Model as LogOnModel;
                Assert.AreEqual("LogOn", viewResult.ViewName);
                Assert.IsTrue(ArePropertiesEqual(expected, actual), $"\nExpected: <{expected}>.\nActual: <{actual}>.");
            }

            // post LogOn
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
                sut.ControllerContext = CreateControllerContext();

                var homeController = SutAs<HomeController>();
                homeController.MembershipService = SetupMock<IMembershipService>().SetupToReturnCertainResultWhenCallingValidateUser(default(string), default(string), ValidationResult.Failure)
                                                                                  .Object;
                var model = new LogOnModel();

                // Act
                var result = SutAs<HomeController>().LogOn(model);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
            }

            [TestMethod]
            public void Post_Logon_Called_With_Bare_Model_And_Good_Credentials__Returns_EmptyResult()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();
                var homeController = SutAs<HomeController>();
                homeController.AuthenticationProvider = new Mock<IAuthenticationProvider>().Object;
                homeController.Url = new Mock<UrlHelper>().Object;

                const string username = "username";
                const string password = "password";
                homeController.MembershipService = SetupMock<IMembershipService>().SetupToReturnCertainResultWhenCallingValidateUser(username, password, ValidationResult.Success)
                                                                                  .Object;
                var model = new LogOnModel { Username = username, Password = password };

                // Act
                var result = homeController.LogOn(model);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(EmptyResult));
            }

            [TestMethod]
            public void Post_Logon_Called_With_Bare_Model_And_No_Authorization__Returns_RedirectResult()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();
                const string username = "username";
                const string password = "password";
                var homeController = SutAs<HomeController>();
                homeController.MembershipService = SetupMock<IMembershipService>().SetupToReturnCertainResultWhenCallingValidateUser(username, password, ValidationResult.NotAuthorized)
                                                                                  .Object;

                var model = new LogOnModel { Username = username, Password = password };

                // Act
                var result = homeController.LogOn(model);

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
                sut.ControllerContext = CreateControllerContext();
                const string username = "username";
                const string password = "password";
                var model = new LogOnModel { Username = username, Password = password };
                var homeController = SutAs<HomeController>();
                homeController.MembershipService = SetupMock<IMembershipService>().SetupToReturnCertainResultWhenCallingValidateUser(username, password, ValidationResult.Failure)
                                                                                  .Object;
                // Act
                var result = homeController.LogOn(model);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
                Assert.IsFalse(sut.ModelState.IsValid);
                Assert.AreEqual(Resources.Home_LogOn_UsernamePasswordIncorrect, sut.ModelState[""].Errors[0].ErrorMessage);
            }

            // get LogOff
            [TestMethod]
            public void Get_LogOff_Called_Without_AuthenticationProvider__Throws_NullReferenceException()
            {
                try
                {
                    SutAs<HomeController>().LogOff();
                }
                catch (NullReferenceException)
                {
                    return;
                }
                Assert.Fail();
            }

            [TestMethod]
            public void Get_LogOff_Called_With_AuthenticationProvider__Return_RedirectToAction_Home_Index()
            {
                // Arrange
                var homeController = SutAs<HomeController>();
                var authenticationProviderMock = new Mock<IAuthenticationProvider>();
                homeController.AuthenticationProvider = authenticationProviderMock.Object;

                // Act
                var result = homeController.LogOff();

                // Assert
                var redirectToAction = AssertAndGetRedirectToRouteResult(result);
                Assert.AreEqual(2, redirectToAction.RouteValues.Count);
                var routeValuesEnumerator = redirectToAction.RouteValues.GetEnumerator();
                try
                {
                    routeValuesEnumerator.MoveNext();
                    Assert.AreEqual("action", routeValuesEnumerator.Current.Key);
                    Assert.AreEqual("Index", routeValuesEnumerator.Current.Value);
                    routeValuesEnumerator.MoveNext();
                    Assert.AreEqual("controller", routeValuesEnumerator.Current.Key);
                    Assert.AreEqual("Home", routeValuesEnumerator.Current.Value);
                }
                finally
                {
                    routeValuesEnumerator.Dispose();
                }
            }

            // get Unauthorized
            [TestMethod]
            public void Get_Unauthorized__Returns_ViewResult()
            {
                // Arrange

                // Act
                var result = SutAs<HomeController>().Unauthorized();

                // Assert
                var viewResult = AssertAndGetViewResult(result);
                Assert.AreEqual("", viewResult.ViewName);
            }

            // get ChangeCulture
            [TestMethod]
            public void Get_ChangeCulture_Called_With_Default_Parameters__Throws_ArgumentNullException()
            {
                // Arrange

                try
                {
                    // Act
                    SutAs<HomeController>().ChangeCulture(default(string), default(string));
                }
                catch (ArgumentNullException)
                {
                    return;
                }

                // Assert
                Assert.Fail();
            }

            [TestMethod]
            public void Get_ChangeCulture_Called_With_Empty_Parameters_With_No_Arrange__Throws_NullReferenceException()
            {
                // Arrange

                // Act
                try
                {
                    SutAs<HomeController>().ChangeCulture(string.Empty, string.Empty);
                }
                catch (NullReferenceException)
                {
                    return;
                }

                // Assert
                Assert.Fail();
            }

            [TestMethod]
            public void Get_ChangeCulture_Called_With_Empty_Parameters_With_Arranged_Session__Throws_ArgumentException()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();
                httpContextMock.SetupGet(c => c.Session)
                               .Returns(new Mock<HttpSessionStateBase>().Object);

                // Act
                try
                {
                    SutAs<HomeController>().ChangeCulture(string.Empty, string.Empty);
                }
                catch (ArgumentException)
                {
                    return;
                }

                // Assert
                Assert.Fail();
            }

            [TestMethod]
            public void Get_ChangeCulture_Called_With_Empty_Lang_And_Non_Empty_ReturnUrl_With_Arranged_Session__Returns_RedirectResult_With_ReturnUrl()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();
                httpContextMock.SetupGet(c => c.Session)
                               .Returns(new Mock<HttpSessionStateBase>().Object);

                // Act
                var result = SutAs<HomeController>().ChangeCulture(string.Empty, "ReturnUrl");

                // Assert
                var redirectResult = AssertAndGetRedirectResult(result);
                Assert.AreEqual("ReturnUrl", redirectResult.Url);
            }

            // get Diagnostics
            [TestMethod]
            public void Get_Diagnostics_Without_Arrange__Throws_NullReferenceException()
            {
                // Arrange

                // Act
                try
                {
                    SutAs<HomeController>().Diagnostics();
                }
                catch (NullReferenceException)
                {
                    return;
                }

                // Assert
                Assert.Fail();
            }

            [TestMethod]
            public void Get_Diagnostics_With_ControllerContext_Arrange__Returns_ViewResult_You_can_only_run_the_diagnostics_locally_to_the_server()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();

                // Act
                var result = SutAs<HomeController>().Diagnostics();

                // Assert
                var contentResult = AssertAndGetContentResult(result);
                Assert.IsNull(contentResult.ContentType);
                Assert.AreEqual("You can only run the diagnostics locally to the server", contentResult.Content);
            }

            [TestMethod]
            public void Get_Diagnostics_With_ControllerContext_Arrange__Returns_ViewResult()
            {
                // Arrange
                sut.ControllerContext = CreateControllerContext();
                requestMock.SetupGet(r => r.IsLocal)
                           .Returns(true);
                ArrangeUserConfiguration();
                var diagnosticReporterMock = SetupMock<DiagnosticReporter>();
                diagnosticReporterMock.SetupGet(d => d.PathResolver)
                                      .Returns(new Mock<IPathResolver>().Object);
                var homeController = SutAs<HomeController>();
                homeController.DiagnosticReporter = diagnosticReporterMock.Object;

                // Act
                var result = homeController.Diagnostics();

                // Assert
                var contentResult = AssertAndGetContentResult(result);
                Assert.AreEqual("text/plain", contentResult.ContentType);
                Assert.AreEqual(Encoding.UTF8, contentResult.ContentEncoding);
                Assert.IsFalse(string.IsNullOrEmpty(contentResult.Content));
            }
        }
    }
}
