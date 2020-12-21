using Bonobo.Git.Server.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bonobo.Git.Server.Test.Unit
{
    [TestClass]
    public class UserConfigurationTests
    {
        [TestMethod]
        public void UserConfiguration_Current_Can_Be_Used_After_Preparing_PathResolver()
        {
            Mock<IPathResolver> pathResolverMock = new Mock<IPathResolver>();
            pathResolverMock.Setup(pr => pr.ResolveWithConfiguration(It.IsAny<string>()))
                            .Returns("BonoboTestConfig.config");
            UserConfiguration.PathResolver = pathResolverMock.Object;
            Assert.IsNotNull(UserConfiguration.Current);
        }
    }
}
