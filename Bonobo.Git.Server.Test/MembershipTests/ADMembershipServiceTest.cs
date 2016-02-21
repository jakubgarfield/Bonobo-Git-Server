using System.Configuration;
using System.IO;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests
{
    // This doesn't run because it needs someone with an AD server to test it against
    [TestClass, Ignore]
    public class ADMembershipServiceTest : MembershipServiceTestBase
    {
        [TestInitialize]
        public void Initialize()
        {
            ConfigurationManager.AppSettings["ActiveDirectoryDefaultDomain"] = "xxx";
            ConfigurationManager.AppSettings["ActiveDirectoryMemberGroupName"] = "users";
            ConfigurationManager.AppSettings["ActiveDirectoryBackendPath"] = Path.Combine(Path.GetTempPath(), "AdTest");
            ActiveDirectorySettings.LoadSettings();
            ADBackend.ResetSingletonForTest();

            ConfigurationManager.AppSettings["ActiveDirectoryRoleMapping"] = "Administrator=Users";
            ConfigurationManager.AppSettings["ActiveDirectoryTeamMapping"] = "Developers=Users";

            _service = new ADMembershipService();
        }
    }
}