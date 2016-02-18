using System.Configuration;
using System.IO;
using System.Web.UI.WebControls.WebParts;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test
{
    [TestClass, Ignore]
    public class ADMembershipServiceTest : MembershipServiceTestBase
    {
        [TestInitialize]
        public void Initialize()
        {
            ConfigurationManager.AppSettings["ActiveDirectoryDefaultDomain"] = "perception.indcomp.co.uk";
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