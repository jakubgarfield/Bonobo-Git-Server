using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Test.Unit
{
    public partial class ControllerTests
    {
        [TestClass]
        public class SettingsControllerTests : ControllerTests
        {
            [TestInitialize]
            public void TestInitialize()
            {
                sut = new SettingsController();
                ConfigurationManager.AppSettings["demoModeActive"] = "false";
                // AuthenticationSettings class controller needs to run again because we changed the demoModelActive appSetting
                ReinitializeStaticClass(typeof(AuthenticationSettings));
            }

            [TestMethod]
            public void Get_Index_Called_With_Uninitialized_Configuration__Throws_TypeInitializationException_Or_DirectoryNotFoundException()
            {
                // Arrange
                ArrangeUserConfiguration();

                // Act
                var result = SutAs<SettingsController>().Index();

                // Assert
                Assert.IsNotNull(result);
            }

            [TestMethod]
            public void Get_Index_Called_With_Initialized_Configuration__Returns_ViewResult()
            {
                // Arrange
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

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<SettingsController>().Index(default(GlobalSettingsModel)));
            }

            [TestMethod]
            public void Post_Index_Called_With_Unbound_Model__Throws_NullReferenceException()
            {
                // Arrange
                ArrangeUserConfiguration();

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<SettingsController>().Index(new GlobalSettingsModel()));
            }

            [TestMethod]
            public void Post_Index_Called_With_Invalid_Model_And_ControllerContext_Set__Returns_ViewResult()
            {
                // Arrange
                ArrangeUserConfiguration();
                sut.ControllerContext = CreateControllerContext();
                httpContextMock.SetupGet(hc => hc.Server)
                               .Returns(new Mock<HttpServerUtilityBase>().Object);
                var model = new GlobalSettingsModel();
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
                sut.ControllerContext = CreateControllerContext();
                httpContextMock.SetupGet(hc => hc.Server)
                               .Returns(new Mock<HttpServerUtilityBase>().Object);
                var model = new GlobalSettingsModel { RepositoryPath = "-" };
                BindModelToController(model);

                // Act
                var result = SutAs<SettingsController>().Index(model);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
                Assert.IsFalse(sut.ModelState.IsValid);
                Assert.AreEqual(1, sut.ModelState.Count);
            }
        }
    }
}
