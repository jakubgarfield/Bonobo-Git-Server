﻿using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace Bonobo.Git.Server.Test.IntegrationTests.Controller
{
    using ITH = IntegrationTestHelpers;
    using TC = TestCategories;

    [TestClass]
    public class RepositoryControllerTests : IntegrationTestBase
    {
        [TestMethod, TestCategory(TC.IntegrationTest), TestCategory(TC.StorageInternal)]
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

        [TestMethod, TestCategory(TC.IntegrationTest)]
        public void CreateDuplicateRepoNameDifferentCaseNotAllowed()
        {
            var reponame = ITH.MakeName();
            ITH.CreateRepositoryOnWebInterface(reponame);

            app.NavigateTo<RepositoryController>(c => c.Create());
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame.ToUpper())
                .Submit();

            var field = app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name);
            field.ShouldBeInvalid();
            Assert.AreEqual(Resources.Validation_Duplicate_Name, field.HasValidationMessage().Text);
        }

        [TestMethod, TestCategory(TC.IntegrationTest)]
        public void CreateDuplicateRepoNameNotAllowed()
        {
            var reponame = ITH.MakeName();
            ITH.CreateRepositoryOnWebInterface(reponame);

            app.NavigateTo<RepositoryController>(c => c.Create());
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame)
                .Submit();

            var field = app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name);
            field.ShouldBeInvalid();
            Assert.AreEqual(Resources.Validation_Duplicate_Name, field.HasValidationMessage().Text);
        }

        [TestMethod, TestCategory(TC.IntegrationTest)]
        public void RenameRepoToExistingRepoNameNotAllowed()
        {
            var reponame = ITH.MakeName();
            var otherreponame = ITH.MakeName(reponame + "_other");
            ITH.CreateRepositoryOnWebInterface(reponame);
            var id2 = ITH.CreateRepositoryOnWebInterface(otherreponame);

            app.NavigateTo<RepositoryController>(c => c.Edit(id2));
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame)
                .Submit();

            var field = app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name);
            field.ShouldBeInvalid();
            var validationmsg = field.HasValidationMessage();
            Assert.AreEqual(Resources.Validation_Duplicate_Name, validationmsg.Text);
        }

        [TestMethod, TestCategory(TC.IntegrationTest)]
        public void RenameRepoToExistingRepoNameNotAllowedDifferentCase()
        {
            var reponame = ITH.MakeName();
            var otherreponame = ITH.MakeName(reponame + "_other");
            ITH.CreateRepositoryOnWebInterface(reponame);
            var id2 = ITH.CreateRepositoryOnWebInterface(otherreponame);

            app.NavigateTo<RepositoryController>(c => c.Edit(id2));
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame.ToUpper())
                .Submit();

            var field = app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name);
            field.ShouldBeInvalid();
            Assert.AreEqual(Resources.Validation_Duplicate_Name, field.HasValidationMessage().Text);
        }

        [TestMethod, TestCategory(TC.IntegrationTest)]
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

        [TestMethod, TestCategory(TC.IntegrationTest)]
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
            app.WaitForElementToBeVisible(By.CssSelector("div.validation-summary-errors"), TimeSpan.FromSeconds(1));
            ITH.AssertThatValidationErrorContains(Resources.Repository_Edit_CantRemoveYourself);
        }

        [TestMethod, TestCategory(TC.IntegrationTest)]
        public void RepoAnonPushDefaultSettingsForRepoCreationShouldBeGlobal()
        {
            app.NavigateTo<RepositoryController>(c => c.Create());
            var form = app.FindFormFor<RepositoryDetailModel>();
            var select = new SelectElement(form.Field(f => f.AllowAnonymousPush).Field);

            Assert.AreEqual(RepositoryPushMode.Global.ToString("D"), select.SelectedOption.GetAttribute("value"));
        }

        [TestMethod, TestCategory(TC.IntegrationTest)]
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

        [TestMethod, TestCategory(TC.IntegrationTest)]
        public void DoesNotAcceptBrokenRegexForLinks()
        {
            app.NavigateTo<SettingsController>(c => c.Index());
            app.FindFormFor<GlobalSettingsModel>()
                .Field(f => f.LinksRegex).SetValueTo("\\")
                .Submit();

            app.FindFormFor<GlobalSettingsModel>()
                .Field(f => f.LinksRegex).ShouldBeInvalid();
        }

        [TestMethod, TestCategory(TC.IntegrationTest)]
        public void RepoNameEnsureDuplicationDetectionAsYouTypeWorksOnCreation()
        {
            var reponame = ITH.MakeName();
            ITH.CreateRepositoryOnWebInterface(reponame);

            app.NavigateTo<RepositoryController>(c => c.Create());
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame)
                .Field(f => f.Description).Click(); // Set focus

            var validation = app.WaitForElementToBeVisible(By.CssSelector("input#Name~span.field-validation-error>span"), TimeSpan.FromSeconds(1), true);
            Assert.AreEqual(Resources.Validation_Duplicate_Name, validation.Text);

            var input = app.Browser.FindElementByCssSelector("input#Name");
            Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
        }

        [TestMethod, TestCategory(TC.IntegrationTest)]
        public void RepoNameEnsureDuplicationDetectionAsYouTypeWorksOnEdit()
        {
            var reponame = ITH.MakeName();
            var otherreponame = ITH.MakeName(reponame + "_other");
            ITH.CreateRepositoryOnWebInterface(reponame);
            var id2 = ITH.CreateRepositoryOnWebInterface(otherreponame);

            app.NavigateTo<RepositoryController>(c => c.Edit(id2));
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame)
                .Field(f => f.Description).Click(); // Set focus

            var validation = app.WaitForElementToBeVisible(By.CssSelector("input#Name~span.field-validation-error>span"), TimeSpan.FromSeconds(1), true);
            Assert.AreEqual(Resources.Validation_Duplicate_Name, validation.Text);

            var input = app.Browser.FindElementByCssSelector("input#Name");
            Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
        }

        [TestMethod, TestCategory(TC.IntegrationTest)]
        public void RepoNameEnsureDuplicationDetectionStillAllowsEditOtherProperties()
        {
            var reponame = ITH.MakeName();
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

        [TestMethod, TestCategory(TC.IntegrationTest)]
        public void InvalidLinkifyRegexAsYouTypeInRepository()
        {
            var reponame = ITH.MakeName();
            var repo_id = ITH.CreateRepositoryOnWebInterface(reponame);
            var brokenRegex = @"\";

            app.NavigateTo<RepositoryController>(c => c.Edit(repo_id));
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.LinksUseGlobal).Click()
                .Field(f => f.LinksRegex).SetValueTo(brokenRegex)
                .Field(f => f.Description).Click(); // Set focus

            var validation = app.WaitForElementToBeVisible(By.CssSelector("input#LinksRegex~span.field-validation-error>span"), TimeSpan.FromSeconds(1), true);
            Assert.IsTrue(validation.Text.Contains(Resources.Validation_Invalid_Regex.Replace("{0}", "")));

            var input = app.Browser.FindElementByCssSelector("input#LinksRegex");
            Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
        }

        [TestMethod, TestCategory(TC.IntegrationTest)]
        public void AnonymousPushModeNotAcceptInvalidValueWhenEditingRepo()
        {

            var repo_id = ITH.CreateRepositoryOnWebInterface(ITH.MakeName());
            app.NavigateTo<RepositoryController>(c => c.Edit(repo_id));
            var form = app.FindFormFor<RepositoryDetailModel>();
            ITH.SetCheckbox(form.Field(f => f.AllowAnonymous).Field, true);
            var select = new SelectElement(form.Field(f => f.AllowAnonymousPush).Field);

            select.SelectByValue(((int)RepositoryPushMode.Global).ToString());

            ITH.SetElementAttribute(select.Options[(int)RepositoryPushMode.Global], "value", "47");
            form.Submit();

            ITH.AssertThatValidationErrorContains(Resources.Repository_Edit_InvalidAnonymousPushMode);
        }

        [TestMethod, TestCategory(TC.IntegrationTest)]
        public void AnonymousPushModeNotAcceptInvalidValueWhenCreatingRepo()
        {

            app.NavigateTo<RepositoryController>(c => c.Create());
            var form = app.FindFormFor<RepositoryDetailModel>();
            form.Field(f => f.Name).SetValueTo(ITH.MakeName());
            ITH.SetCheckbox(form.Field(f => f.AllowAnonymous).Field, true);
            var select = new SelectElement(form.Field(f => f.AllowAnonymousPush).Field);

            select.SelectByValue(((int)RepositoryPushMode.Global).ToString());

            ITH.SetElementAttribute(select.Options[(int)RepositoryPushMode.Global], "value", "47");
            form.Submit();

            ITH.AssertThatValidationErrorContains(Resources.Repository_Edit_InvalidAnonymousPushMode);
        }

    }
}
