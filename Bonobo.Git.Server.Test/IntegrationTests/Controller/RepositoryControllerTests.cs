using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Data;
using System.Linq;
using OpenQA.Selenium.Support.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting; 
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;

namespace Bonobo.Git.Server.Test.IntegrationTests.Controller
{
    using ITH = IntegrationTestHelpers;

    [TestClass]
    public class RepositoryControllerTests : IntegrationTestBase
    {
        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void EnsureCheckboxesStayCheckOnCreateError()
        {
            var user = ITH.CreateUsers(1).Single();
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
            var reponame = ITH.MakeName();
            var id1 = ITH.CreateRepositoryOnWebInterface(reponame);

            app.NavigateTo<RepositoryController>(c => c.Create());
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame.ToUpper())
                .Submit();

            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).ShouldBeInvalid();

     //       ITH.DeleteRepository(id1);

        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void CreateDuplicateRepoNameNotAllowed()
        {
            var reponame = ITH.MakeName();
            var id1 = ITH.CreateRepositoryOnWebInterface(reponame);

            app.NavigateTo<RepositoryController>(c => c.Create());
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame)
                .Submit();

            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).ShouldBeInvalid();

      //      ITH.DeleteRepository(id1);
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RenameRepoToExistingRepoNameNotAllowed()
        {
            var reponame = ITH.MakeName();
            var otherreponame = ITH.MakeName(reponame + "_other");
            var id1 = ITH.CreateRepositoryOnWebInterface(reponame);
            var id2 = ITH.CreateRepositoryOnWebInterface(otherreponame);

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
            var reponame = ITH.MakeName();
            var otherreponame = ITH.MakeName(reponame + "_other");
            var id1 = ITH.CreateRepositoryOnWebInterface(reponame);
            var id2 = ITH.CreateRepositoryOnWebInterface(otherreponame);

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
            var repoId = ITH.CreateRepositoryOnWebInterface(ITH.MakeName());

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
            var user = ITH.CreateUsers().Single();

            var repoId = ITH.CreateRepositoryOnWebInterface(ITH.MakeName());

            app.NavigateTo<RepositoryController>(c => c.Edit(repoId));
            var form = app.FindFormFor<RepositoryDetailModel>();

            // Set User0 to be admin for this repo
            var adminBox =
                form.WebApp.Browser.FindElementsByCssSelector(
                    string.Format("input[name=PostedSelectedAdministrators][value=\"{0}\"]", user.Id)).Single();
            ITH.SetCheckbox(adminBox, true);
            form.Submit();
            ITH.AssertThatNoValidationErrorOccurred();

            // Now, log in as the ordinary user
            ITH.LoginAsUser(user);

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
        public void SettingsAcceptEmptyStringForRegex()
        {
            ITH.SetGlobalSetting(g => g.LinksRegex, "some_value");
            app.NavigateTo<SettingsController>(c => c.Index());
            app.FindFormFor<GlobalSettingsModel>()
                .Field(f => f.LinksRegex).SetValueTo("")
                .Submit();

            var field = app.FindFormFor<GlobalSettingsModel>()
                .Field(f => f.LinksRegex);
            field.ValueShouldEqual("");
            Assert.AreEqual(false, field.Field.GetAttribute("class").Contains("valid"));
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void DoesNotAcceptBrokenRegexForLinks()
        {
            app.NavigateTo<SettingsController>(c => c.Index());
            app.FindFormFor<GlobalSettingsModel>()
                .Field(f => f.LinksRegex).SetValueTo("\\")
                .Submit();

            app.FindFormFor<GlobalSettingsModel>()
                .Field(f => f.LinksRegex).ShouldBeInvalid();
        }
    }
}

