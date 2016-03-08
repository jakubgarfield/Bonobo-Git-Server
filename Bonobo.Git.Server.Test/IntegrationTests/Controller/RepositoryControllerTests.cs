﻿using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Support.UI;
using SpecsFor.Mvc;
using System.Linq;
using System.Reflection;

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
            using (var id1 = ITH.CreateRepositoryOnWebInterface(app))
            {

                app.NavigateTo<RepositoryController>(c => c.Create());
                app.FindFormFor<RepositoryDetailModel>()
                    .Field(f => f.Name).SetValueTo(id1.Name.ToUpper())
                    .Submit();

                app.FindFormFor<RepositoryDetailModel>()
                    .Field(f => f.Name).ShouldBeInvalid();

            }
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void CreateDuplicateRepoNameNotAllowed()
        {
            using (var id1 = ITH.CreateRepositoryOnWebInterface(app))
            {

                app.NavigateTo<RepositoryController>(c => c.Create());
                app.FindFormFor<RepositoryDetailModel>()
                    .Field(f => f.Name).SetValueTo(id1.Name)
                    .Submit();

                app.FindFormFor<RepositoryDetailModel>()
                    .Field(f => f.Name).ShouldBeInvalid();
            }
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RenameRepoToExistingRepoNameNotAllowed()
        {
            var reponame = MethodBase.GetCurrentMethod().Name;
            using (var id1 = ITH.CreateRepositoryOnWebInterface(app, reponame))
            using (var id2 = ITH.CreateRepositoryOnWebInterface(app, reponame + "_other"))
            {
                app.NavigateTo<RepositoryController>(c => c.Edit(id2));
                app.FindFormFor<RepositoryDetailModel>()
                    .Field(f => f.Name).SetValueTo(reponame)
                    .Submit();

                app.FindFormFor<RepositoryDetailModel>()
                    .Field(f => f.Name).ShouldBeInvalid();

            }
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RenameRepoToExistingRepoNameNotAllowedDifferentCase()
        {
            var reponame = MethodBase.GetCurrentMethod().Name;
            using (var id1 = ITH.CreateRepositoryOnWebInterface(app, reponame))
            using (var id2 = ITH.CreateRepositoryOnWebInterface(app, reponame + "_other"))
            {

                app.NavigateTo<RepositoryController>(c => c.Edit(id2));
                app.FindFormFor<RepositoryDetailModel>()
                    .Field(f => f.Name).SetValueTo(reponame.ToUpper())
                    .Submit();

                app.FindFormFor<RepositoryDetailModel>()
                    .Field(f => f.Name).ShouldBeInvalid();

            }
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RepositoryCanBeSavedBySysAdminWithoutHavingAnyRepoAdmins()
        {
            using (var repoId = ITH.CreateRepositoryOnWebInterface(app))
            {
                app.NavigateTo<RepositoryController>(c => c.Edit(repoId));
                var form = app.FindFormFor<RepositoryDetailModel>();

                // Turn off all the admin checkboxes and save the form 
                var chkboxes = form.WebApp.Browser.FindElementsByCssSelector("input[name=PostedSelectedAdministrators]");
                ITH.SetCheckboxes(chkboxes, false);

                form.Submit();
                ITH.AssertThatNoValidationErrorOccurred(app);
            }
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RepoAdminCannotRemoveThemselves()
        {
            var user = ITH.CreateUsers(app).Single();
            var repoId = ITH.CreateRepositoryOnWebInterface(app);

            try
            {
                app.NavigateTo<RepositoryController>(c => c.Edit(repoId));
                var form = app.FindFormFor<RepositoryDetailModel>();

                // Set User0 to be admin for this repo
                var adminBox = form.WebApp.Browser.FindElementsByCssSelector(string.Format("input[name=PostedSelectedAdministrators][value=\"{0}\"]", user.Id)).Single();
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
            }
            finally
            {
                // we are logged in as user.
                // Relog in as admin to ensure we can delete the repo and user.
                ITH.Login(app);
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

