using Bonobo.Git.Server.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.Unit
{
    public partial class PRGPatternTests
    {
        [TestClass]
        public class RepositoryControllerTests : PRGPatternTests
        {
            [TestInitialize]
            public void TestInitialize()
            {
                sut = new RepositoryController();
            }

            // get Edit
            [TestMethod]
            public void Get_Edit_Executed_With_Null_Parameters__Throws_NullReferenceException()
            {
                // Arrange
                try
                {
                    // Act
                    SutAs<RepositoryController>().Edit(null);
                }
                catch (NullReferenceException)
                {
                    return;
                }
                // Assert
                Assert.Fail();
            }

            // post Edit
            // get Create
            // post Create
            // get Delete
            // post Delete
            // get Clone
            // post Clone
        }
    }
}
