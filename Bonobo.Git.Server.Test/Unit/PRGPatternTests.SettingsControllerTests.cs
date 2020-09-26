using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Configuration;
using System.IO;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Test.UnitTests
{
    public partial class PRGPatternTests
    {
        [TestClass]
        public class SettingsControllerTests : PRGPatternTests
        {
            [TestInitialize]
            public void TestInitialize()
            {
                sut = new SettingsController();
            }

            [TestMethod]
            public void Get_Index_Called_With_Initialized_Configuration__Throws_No_Exceptions()
            {
                // Arrange (REFACTOR THIS CODE WHEN START CHANGES ON PRODUCTION CODE SO WE DON'T HAVE REPETITIONS)
                ArrangeUserConfiguration();

                // Act
                var result = SutAs<SettingsController>().Index();

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));

                var viewResult = result as ViewResult;

                Assert.IsNotNull(viewResult.Model);
                Assert.IsInstanceOfType(viewResult.Model, typeof(GlobalSettingsModel));
            }

            [TestMethod]
            public void Post_Index_Called_With_Null_Model__Throws_NullReferenceException()
            {
                // Arrange
                ArrangeUserConfiguration();

                try
                {
                    // Act
                    SutAs<SettingsController>().Index(null);
                }
                catch (NullReferenceException)
                {
                    return;
                }

                // Assert
                Assert.Fail();
            }

            [TestMethod]
            public void Post_Index_Called_With_Unbound_Model__Throws_NullReferenceException()
            {
                // Arrange
                ArrangeUserConfiguration();

                try
                {
                    // Act
                    SutAs<SettingsController>().Index(new GlobalSettingsModel());

                }
                catch (NullReferenceException)
                {
                    return;
                }

                // Assert
                Assert.Fail();
            }

            [TestMethod]
            public void Post_Index_Called_With_Invalid_Model_And_ControllerContext_Set__Returns_ViewResult()
            {
                // Arrange
                ArrangeUserConfiguration();
                sut.ControllerContext = CreateControllerContextFromPrincipal(new Mock<IPrincipal>().Object);
                httpContextMock.SetupGet(hc => hc.Server)
                               .Returns(new Mock<HttpServerUtilityBase>().Object);
                GlobalSettingsModel model = new GlobalSettingsModel();
                BindModelToController(model);

                // Act
                var result = SutAs<SettingsController>().Index(model);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
            }

            [TestMethod]
            public void Post_Index_Called_With_Valid_Model_With_Non_Existent_Folder_And_ControllerContext_Set__Returns_ViewResult_With_Errors()
            {
                // Arrange
                ArrangeUserConfiguration();
                sut.ControllerContext = CreateControllerContextFromPrincipal(new Mock<IPrincipal>().Object);
                httpContextMock.SetupGet(hc => hc.Server)
                               .Returns(new Mock<HttpServerUtilityBase>().Object);
                GlobalSettingsModel model = new GlobalSettingsModel { RepositoryPath = "-" };
                BindModelToController(model);

                // Act
                var result = SutAs<SettingsController>().Index(model);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
                Assert.IsFalse(sut.ModelState.IsValid);
                Assert.AreEqual(1, sut.ModelState.Count);
            }

            private static void ArrangeUserConfiguration()
            {
                var configFileName = Path.Combine(Path.GetTempFileName(), "BonoboTestConfig.xml");
                ConfigurationManager.AppSettings["UserConfiguration"] = configFileName;
                UserConfiguration.InitialiseForTest();
            }
        }
    }
}
