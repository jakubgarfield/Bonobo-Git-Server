using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests.ADTests
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
        }

        [TestCleanup]
        public void Cleanup()
        {
            _testSupport.Dispose();
        }
    }
}