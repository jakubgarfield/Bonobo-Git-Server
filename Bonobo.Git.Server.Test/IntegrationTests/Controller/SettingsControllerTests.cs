﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpecsFor.Mvc;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;
using Bonobo.Git.Server.Controllers;
using OpenQA.Selenium.Support.UI;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Test.Integration.Web;

namespace Bonobo.Git.Server.Test.IntegrationTests.Controller
{
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
    }
}
