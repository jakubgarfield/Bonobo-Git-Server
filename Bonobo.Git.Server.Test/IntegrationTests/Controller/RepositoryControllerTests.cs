using System.Linq;
using System.Reflection;
using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.IntegrationTests.Controller
{
    using OpenQA.Selenium.Support.UI; 

    [TestClass]
    public class RepositoryControllerTests : IntegrationTestBase
    {
        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void EnsureCheckboxesStayCheckOnCreateError()
        {
            ITH.CreateUsers(1);
            app.NavigateTo<RepositoryController>(c => c.Create());
            var form = app.FindFormFor<RepositoryDetailModel>();
            var chkboxes = form.WebApp.Browser.FindElementsByCssSelector("form.pure-form>fieldset>div.pure-control-group.checkboxlist>input");
            foreach (var chk in chkboxes)
            {
                ITH.SetCheckbox(chk, true);
            }
            form.Submit();

            form = app.FindFormFor<RepositoryDetailModel>();
            chkboxes = form.WebApp.Browser.FindElementsByCssSelector("form.pure-form>fieldset>div.pure-control-group.checkboxlist>input");
            foreach (var chk in chkboxes)
            {
                Assert.AreEqual(true, chk.Selected, "A message box was unselected eventhough we selected all!");
            }
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void CreateDuplicateRepoNameDifferentCaseNotAllowed()
        {
            var reponame = ITH.MakeRepoName(MethodBase.GetCurrentMethod());
            ITH.CreateRepositoryOnWebInterface(reponame);

            app.NavigateTo<RepositoryController>(c => c.Create());
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame.ToUpper())
                .Submit();

            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).ShouldBeInvalid();
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void CreateDuplicateRepoNameNotAllowed()
        {
            var reponame = ITH.MakeRepoName(MethodBase.GetCurrentMethod());
            ITH.CreateRepositoryOnWebInterface(reponame);

            app.NavigateTo<RepositoryController>(c => c.Create());
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame)
                .Submit();

            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).ShouldBeInvalid();
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RenameRepoToExistingRepoNameNotAllowed()
        {
            var reponame = ITH.MakeRepoName(MethodBase.GetCurrentMethod());
            ITH.CreateRepositoryOnWebInterface(reponame);
            var id2 = ITH.CreateRepositoryOnWebInterface("other_name");

            app.NavigateTo<RepositoryController>(c => c.Edit(id2));
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame)
                .Submit();
            
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).ShouldBeInvalid();
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RenameRepoToExistingRepoNameNotAllowedDifferentCase()
        {
            var reponame = ITH.MakeRepoName(MethodBase.GetCurrentMethod());
            ITH.CreateRepositoryOnWebInterface(reponame);
            var id2 = ITH.CreateRepositoryOnWebInterface("other_name");

            app.NavigateTo<RepositoryController>(c => c.Edit(id2));
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame.ToUpper())
                .Submit();

            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).ShouldBeInvalid();
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RepositoryCanBeSavedBySysAdminWithoutHavingAnyRepoAdmins()
        {
            var repoId = ITH.CreateRepositoryOnWebInterface("Repo");

            app.NavigateTo<RepositoryController>(c => c.Edit(repoId));
            var form = app.FindFormFor<RepositoryDetailModel>();

            // Turn off all the admin checkboxes and save the form 
            var chkboxes = form.WebApp.Browser.FindElementsByCssSelector("input[name=PostedSelectedAdministrators]");
            ITH.SetCheckboxes(chkboxes, false);

            form.Submit();
            ITH.AssertThatNoValidationErrorOccurred();
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RepoAdminCannotRemoveThemselves()
        {
            var userId = ITH.CreateUsers().Single();
            var repoId = ITH.CreateRepositoryOnWebInterface("Repo");

            app.NavigateTo<RepositoryController>(c => c.Edit(repoId));
            var form = app.FindFormFor<RepositoryDetailModel>();

            // Set User0 to be admin for this repo
            var adminBox = form.WebApp.Browser.FindElementsByCssSelector(string.Format("input[name=PostedSelectedAdministrators][value=\"{0}\"]", userId)).Single();
            ITH.SetCheckbox(adminBox, true);
            form.Submit();
            ITH.AssertThatNoValidationErrorOccurred();

            // Now, log in as the ordinary user
            ITH.LoginAsNumberedUser(0);

            app.NavigateTo<RepositoryController>(c => c.Edit(repoId));
            form = app.FindFormFor<RepositoryDetailModel>();

            var chkboxes = form.WebApp.Browser.FindElementsByCssSelector("input[name=PostedSelectedAdministrators]");
            ITH.SetCheckboxes(chkboxes, false);

            form.Submit();
            ITH.AssertThatValidationErrorContains("You can't remove yourself from administrators");
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RepoAnonPushDefaultSettingsForRepoCreationShouldBeGlobal()
        {
            app.NavigateTo<RepositoryController>(c => c.Create());
            var form = app.FindFormFor<RepositoryDetailModel>();
            var select = new SelectElement(form.Field(f => f.AllowAnonymousPush).Field);

            Assert.AreEqual(RepositoryPushMode.Global.ToString("D"), select.SelectedOption.GetAttribute("value"));
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RepoNameEnsureDuplicationDetectionAsYouTypeWorksOnCreation()
        {
            var reponame = ITH.MakeRepoName(MethodBase.GetCurrentMethod());
            ITH.CreateRepositoryOnWebInterface(reponame);

            app.NavigateTo<RepositoryController>(c => c.Create());
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame)
                .Field(f => f.Description).Click(); // Set focus

            var input = app.Browser.FindElementByCssSelector("input#Name");
            Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RepoNameEnsureDuplicationDetectionAsYouTypeWorksOnEdit()
        {
            var reponame = ITH.MakeRepoName(MethodBase.GetCurrentMethod());
            ITH.CreateRepositoryOnWebInterface(reponame);
            var id2 = ITH.CreateRepositoryOnWebInterface(reponame + "_other");

            app.NavigateTo<RepositoryController>(c => c.Edit(id2));
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame)
                .Field(f => f.Description).Click(); // Set focus

            var input = app.Browser.FindElementByCssSelector("input#Name");
            Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RepoNameEnsureDuplicationDetectionStillAllowsEditOtherProperties()
        {
            var reponame = ITH.MakeRepoName(MethodBase.GetCurrentMethod());
            var repo = ITH.CreateRepositoryOnWebInterface(reponame);

            app.NavigateTo<RepositoryController>(c => c.Edit(repo));
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Description).SetValueTo(reponame + "_other")
                .Submit();
            ITH.AssertThatNoValidationErrorOccurred();

            app.NavigateTo<RepositoryController>(c => c.Edit(repo)); // force refresh
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Description).ValueShouldEqual(reponame + "_other");
        }
    }
}
