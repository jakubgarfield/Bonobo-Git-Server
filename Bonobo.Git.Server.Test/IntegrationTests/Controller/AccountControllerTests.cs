using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Linq;

namespace Bonobo.Git.Server.Test.IntegrationTests.Controller
{
    using ITH = IntegrationTestHelpers;
    using TC = TestCategories;

    public class AccountControllerSpecs
    {
        [TestClass]
        public class AccountControllerTests : IntegrationTestBase
        {

            [TestMethod, TestCategory(TC.IntegrationTest), TestCategory(TC.StorageInternal)]
            public void UserDetailRequiresLogin()
            {
                app.NavigateTo<HomeController>(c => c.LogOff());
                app.UrlShouldMapTo<HomeController>(c => c.LogOn("/Home/Index"));

                app.NavigateTo<AccountController>(c => c.Detail(new Guid("7479fc09-2c0b-4e93-a2cf-5e4bbf6bab4f")));

                app.UrlShouldMapTo<HomeController>(c => c.LogOn("/Account/Detail/7479fc09-2c0b-4e93-a2cf-5e4bbf6bab4f"));
            }

            [TestMethod, TestCategory(TC.IntegrationTest), TestCategory(TC.AuthForms)]
            public void LoginWithoutCredentialFailsWithInvalidMessages()
            {
                app.NavigateTo<HomeController>(c => c.LogOn("/"));
                app.FindFormFor<LogOnModel>()
                    .Field(f => f.Username).SetValueTo("")
                    .Field(f => f.Password).SetValueTo("")
                    .Submit();

                app.FindFormFor<LogOnModel>()
                    .Field(f => f.Username).ShouldBeInvalid();
                app.FindFormFor<LogOnModel>()
                    .Field(f => f.Password).ShouldBeInvalid();
            }

            [TestMethod, TestCategory(TC.IntegrationTest), TestCategory(TC.StorageInternal)]
            public void UsernameEnsureDuplicationDetectionAsYouTypeWorksOnCreation()
            {
                var id1 = ITH.CreateUsers().Single();
                app.NavigateTo<AccountController>(c => c.Create());
                var form = app.FindFormFor<UserCreateModel>()
                    .Field(f => f.Username).SetValueTo(id1.Username)
                    .Field(f => f.Name).Click(); // Set focus

                var validation = app.WaitForElementToBeVisible(By.CssSelector("input#Username~span.field-validation-error>span"), TimeSpan.FromSeconds(1), true);
                Assert.AreEqual(Resources.Validation_Duplicate_Name, validation.Text);

                var input = app.Browser.FindElementByCssSelector("input#Username");
                Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
            }

            [TestMethod, TestCategory(TC.IntegrationTest), TestCategory(TC.StorageInternal)]
            public void UsernameEnsureDuplicationDetectionAsYouTypeWorksOnEdit()
            {
                var ids = ITH.CreateUsers(2).ToList();
                var id1 = ids[0];
                var id2 = ids[1];
                app.NavigateTo<AccountController>(c => c.Edit(id2.Id));
                var form = app.FindFormFor<UserCreateModel>()
                    .Field(f => f.Username).SetValueTo(id1.Username)
                    .Field(f => f.Name).Click(); // Set focus

                var validation = app.WaitForElementToBeVisible(By.CssSelector("input#Username~span.field-validation-error>span"), TimeSpan.FromSeconds(1), true);
                Assert.AreEqual(Resources.Validation_Duplicate_Name, validation.Text);

                var input = app.Browser.FindElementByCssSelector("input#Username");
                Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
            }

            [TestMethod, TestCategory(TC.IntegrationTest), TestCategory(TC.StorageInternal)]
            public void UsernameEnsureDuplicationDetectionStillAllowsEditOtherProperties()
            {
                var ids = ITH.CreateUsers().ToList();
                var id1 = ids[0];
                app.NavigateTo<AccountController>(c => c.Edit(id1.Id));
                app.FindFormFor<UserCreateModel>()
                    .Field(f => f.Name).SetValueTo("somename")
                    .Submit();

                app.NavigateTo<AccountController>(c => c.Edit(id1.Id)); // force refresh
                app.FindFormFor<UserCreateModel>()
                    .Field(f => f.Name).ValueShouldEqual("somename");
            }
        }
    }
}
