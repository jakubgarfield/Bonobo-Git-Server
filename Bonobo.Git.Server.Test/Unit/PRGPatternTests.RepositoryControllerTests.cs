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
