using System;
using System.Linq;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests
{
    [TestClass]
    public class ADMembershipServiceTest : MembershipServiceTestBase
    {
        private ADTestSupport _testSupport;

        [TestInitialize]
        public void Initialize()
        {
            _testSupport = new ADTestSupport();
            _service = new ADMembershipServiceTestFacade(new ADMembershipService(), _testSupport);
            ADBackend.Instance.Users.Add(new UserModel() { Username = "admin", Id = Guid.NewGuid()});
            Assert.AreEqual(1, ADBackend.Instance.Users.Count());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _testSupport.Dispose();
        }
    }
}