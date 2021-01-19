using Bonobo.Git.Server.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
        }
    }
}
