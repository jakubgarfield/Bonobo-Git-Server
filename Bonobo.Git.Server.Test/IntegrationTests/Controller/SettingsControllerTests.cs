using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace Bonobo.Git.Server.Test.IntegrationTests.Controller
{
    using ITH = IntegrationTestHelpers;

    [TestClass]
    public class SettingsControllerTests : IntegrationTestBase
    {
        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void EnsureSelectedLanguageIsSaved()
        {
            app.NavigateTo<SettingsController>(c => c.Index());
            var form = app.FindFormFor<GlobalSettingsModel>();
            var langs = new SelectElement(form.Field(f => f.DefaultLanguage).Field);
            Assert.AreNotEqual("de-DE", langs.SelectedOption.GetAttribute("value"));
            langs.SelectByValue("de-DE");
            form.Submit();

            langs = new SelectElement(app.FindDisplayFor<GlobalSettingsModel>().DisplayFor(f => f.DefaultLanguage));
            Assert.AreEqual("de-DE", langs.SelectedOption.GetAttribute("value"));

            /* Set to english again so it is easier to test later */
            form = app.FindFormFor<GlobalSettingsModel>();
            langs = new SelectElement(form.Field(f => f.DefaultLanguage).Field);
            langs.SelectByValue("en-US");
            form.Submit();
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void InvalidLinkifyRegexAsYouTypeInSettings()
        {
            var brokenRegex = @"\";

            app.NavigateTo<SettingsController>(c => c.Index());
            app.FindFormFor<GlobalSettingsModel>()
                .Field(f => f.LinksRegex).SetValueTo(brokenRegex)
                .Field(f => f.LinksUrl).Click(); // Set focus

            var validation = app.WaitForElementToBeVisible(By.CssSelector("input#LinksRegex~span.field-validation-error>span"), TimeSpan.FromSeconds(1), true);
            Assert.IsTrue(validation.Text.Contains(Resources.Validation_Invalid_Regex.Replace("{0}", "")));

            var input = app.Browser.FindElementByCssSelector("input#LinksRegex");
            Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
        }
    }
}
