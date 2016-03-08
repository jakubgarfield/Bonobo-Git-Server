using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Support.UI;
using SpecsFor.Mvc;
using System.Linq;

namespace Bonobo.Git.Server.Test.IntegrationTests.Controller
{
    using ITH = IntegrationTestHelpers;

    [TestClass]
    public class RepositoryControllerTests
    {
        private static MvcWebApp app;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            app = new MvcWebApp();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            app.Browser.Close();
        }

        [TestInitialize]
        public void InitTests()
        {
            ITH.Login(app);
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void EnsureCheckboxesStayCheckOnCreateError()
        {
            var userId = ITH.CreateUsers(app, 1).Single();
            try
            {
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
            finally
            {
                ITH.DeleteUser(app, userId);
            }

        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void CreateDuplicateRepoNameDifferentCaseNotAllowed()
        {
            var reponame = "A_Nice_Repo";
            var id1 = ITH.CreateRepositoryOnWebInterface(app, reponame);

            app.NavigateTo<RepositoryController>(c => c.Create());
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame.ToUpper())
                .Submit();

            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).ShouldBeInvalid();

            ITH.DeleteRepositoryUsingWebsite(app, id1);

        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void CreateDuplicateRepoNameNotAllowed()
        {
            var reponame = "A_Nice_Repo";
            var id1 = ITH.CreateRepositoryOnWebInterface(app, reponame);

            app.NavigateTo<RepositoryController>(c => c.Create());
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame)
                .Submit();

            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).ShouldBeInvalid();

            ITH.DeleteRepositoryUsingWebsite(app, id1);

        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RenameRepoToExistingRepoNameNotAllowed()
        {
            var reponame = "A_Nice_Repo";
            var id1 = ITH.CreateRepositoryOnWebInterface(app, reponame);
            var id2 = ITH.CreateRepositoryOnWebInterface(app, "other_name");

            app.NavigateTo<RepositoryController>(c => c.Edit(id2));
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame)
                .Submit();

            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).ShouldBeInvalid();

            ITH.DeleteRepositoryUsingWebsite(app, id1);
            ITH.DeleteRepositoryUsingWebsite(app, id2);

        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RenameRepoToExistingRepoNameNotAllowedDifferentCase()
        {
            var reponame = "A_Nice_Repo";
            var id1 = ITH.CreateRepositoryOnWebInterface(app, reponame);
            var id2 = ITH.CreateRepositoryOnWebInterface(app, "other_name");

            app.NavigateTo<RepositoryController>(c => c.Edit(id2));
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame.ToUpper())
                .Submit();

            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).ShouldBeInvalid();

            ITH.DeleteRepositoryUsingWebsite(app, id1);
            ITH.DeleteRepositoryUsingWebsite(app, id2);

        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RepositoryCanBeSavedBySysAdminWithoutHavingAnyRepoAdmins()
        {
            var repoId = ITH.CreateRepositoryOnWebInterface(app, "RepositoryCanBeSavedBySysAdminWithoutHavingAnyRepoAdmins");

            app.NavigateTo<RepositoryController>(c => c.Edit(repoId));
            var form = app.FindFormFor<RepositoryDetailModel>();

            // Turn off all the admin checkboxes and save the form 
            var chkboxes = form.WebApp.Browser.FindElementsByCssSelector("input[name=PostedSelectedAdministrators]");
            ITH.SetCheckboxes(chkboxes, false);

            form.Submit();
            ITH.AssertThatNoValidationErrorOccurred(app);
            ITH.DeleteRepositoryUsingWebsite(app, repoId);
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RepoAdminCannotRemoveThemselves()
        {
            var userId = ITH.CreateUsers(app).Single();

            try
            {
                var repoId = ITH.CreateRepositoryOnWebInterface(app, "Repo");

                app.NavigateTo<RepositoryController>(c => c.Edit(repoId));
                var form = app.FindFormFor<RepositoryDetailModel>();

	            // Set User0 to be admin for this repo
	            var adminBox = form.WebApp.Browser.FindElementsByCssSelector(string.Format("input[name=PostedSelectedAdministrators][value=\"{0}\"]", userId)).Single();
	            ITH.SetCheckbox(adminBox, true);
	            form.Submit();
	            ITH.AssertThatNoValidationErrorOccurred(app);

                // Now, log in as the ordinary user
                ITH.LoginAsNumberedUser(app, 0);

                app.NavigateTo<RepositoryController>(c => c.Edit(repoId));
                form = app.FindFormFor<RepositoryDetailModel>();

                var chkboxes = form.WebApp.Browser.FindElementsByCssSelector("input[name=PostedSelectedAdministrators]");
                ITH.SetCheckboxes(chkboxes, false);

                form.Submit();
                ITH.AssertThatValidationErrorContains(app, "You can't remove yourself from administrators");
                ITH.DeleteRepositoryUsingWebsite(app, repoId);
            }
            finally
            {
                ITH.Login(app);
                ITH.DeleteUser(app, userId);
            }
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RepoAnonPushDefaultSettingsForRepoCreationShouldBeGlobal()
        {
            app.NavigateTo<RepositoryController>(c => c.Create());
            var form = app.FindFormFor<RepositoryDetailModel>();
            var select = new SelectElement(form.Field(f => f.AllowAnonymousPush).Field);

            Assert.AreEqual(RepositoryPushMode.Global.ToString("D"), select.SelectedOption.GetAttribute("value"));
        }
    }
}

