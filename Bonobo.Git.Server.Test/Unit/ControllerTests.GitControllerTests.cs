using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Test.Unit
{
    public partial class ControllerTests
    {
        [TestClass]
        public class GitControllerTests : ControllerTests
        {
            [TestInitialize]
            public void TestInitialize()
            {
                sut = new GitController();
            }

            // Get SecureGetInfoRefs tests
            // Post SecureUploadPack
            // Post SecureReceivePack
            // Get GitUrl
            [TestMethod]
            public void Get_GitUrl_Called_With_Default_RepositoryName_Without_Setting_Any_Mock__Thrown_NullReferenceException()
            {
                // Arrange

                // Act & Assert
                Assert.ThrowsException<NullReferenceException>(() => SutAs<GitController>().GitUrl(default(string)));
            }

            [TestMethod]
            public void Get_GitUrl_Called_Setting_RepositoryName_And_UrlHelper_Mocks___Returns_RedirectResult_With_RepositoryName_In_Resulting_Url()
            {
                // Arrange
                const string ExpectedResultUrl = "url";
                const string RepositoryName = "repositoryName";

                var routeValuesIdProperty = default(string);
                var urlHelperMock = SetupMock<UrlHelper>();

                urlHelperMock.Setup(u => u.Action("Detail", "Repository", Moq.It.IsAny<object>()))
                             .Callback<string, string, object>((action, controller, routeValues) => {
                                 var type = routeValues.GetType();

                                 routeValuesIdProperty = (string)type.GetProperty("id").GetValue(routeValues);
                             })
                             .Returns(ExpectedResultUrl);
                sut.Url = urlHelperMock.Object;

                var gitController = SutAs<GitController>();
                gitController.RepositoryRepository = SetupMock<IRepositoryRepository>().Object;

                // Act 
                var result = gitController.GitUrl(RepositoryName);

                // Assert
                Assert.AreEqual(RepositoryName, routeValuesIdProperty);
                var redirectResult = AssertAndGetRedirectResult(result);
                Assert.AreEqual(ExpectedResultUrl, redirectResult.Url);
            }

            [TestMethod]
            public void Get_GitUrl_Called_Setting_RepositoryName_And_UrlHelper_Mocks_Returning_Id___Returns_RedirectResult_With_Id_In_Resulting_Url()
            {
                // Arrange
                const string ExpectedResultUrl = "url";
                const string RepositoryName = "repositoryName";
                Guid ExpectedGuid = Guid.NewGuid();

                var urlHelperMock = SetupMock<UrlHelper>();
                var gitController = SutAs<GitController>();
                var routeValuesIdProperty = Guid.Empty;

                gitController.RepositoryRepository = SetupMock<IRepositoryRepository>().SetupToReturnAModelWithASpecificIdWhenCallingGetRepositoryMethod(RepositoryName, ExpectedGuid).Object;
                urlHelperMock.Setup(u => u.Action("Detail", "Repository", Moq.It.IsAny<object>()))
                             .Callback<string, string, object>((action, controller, routeValues) => {
                                 var type = routeValues.GetType();

                                 routeValuesIdProperty = (Guid)type.GetProperty("id").GetValue(routeValues);
                             })
                             .Returns(ExpectedResultUrl);
                sut.Url = urlHelperMock.Object;

                // Act 
                var result = gitController.GitUrl(RepositoryName);

                // Assert
                Assert.AreEqual(ExpectedGuid, routeValuesIdProperty);
                var redirectResult = AssertAndGetRedirectResult(result);
                Assert.AreEqual(ExpectedResultUrl, redirectResult.Url);
            }
        }
    }
}
