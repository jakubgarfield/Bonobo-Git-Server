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
    public class HomeControllerSpecs
    {
        [TestClass]
        public class TeamControllerTests : IntegrationTestBase
        {

            [TestMethod, TestCategory(TestCategories.IntegrationTest)]
            public void TeamNameEnsureDuplicationDetectionAsYouTypeWorksOnCreation()
            {
                var id1 = ITH.CreateTeams().Single();
                app.NavigateTo<TeamController>(c => c.Create());
                var form = app.FindFormFor<TeamEditModel>()
                    .Field(f => f.Name).SetValueTo(id1.Name)
                    .Field(f => f.Description).Click(); // Set focus


                var input = app.Browser.FindElementByCssSelector("input#Name");
                Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
            }

            [TestMethod, TestCategory(TestCategories.IntegrationTest)]
            public void TeamNameEnsureDuplicationDetectionAsYouTypeWorksOnEdit()
            {
                var teams = ITH.CreateTeams(2).ToList();
                var id1 = teams[0];
                var id2 = teams[1];
                app.NavigateTo<TeamController>(c => c.Edit(id2.Id));
                var form = app.FindFormFor<TeamEditModel>()
                    .Field(f => f.Name).SetValueTo(id1.Name)
                    .Field(f => f.Description).Click(); // Set focus

                var validation = app.WaitForElementToBeVisible(By.CssSelector("input#Name~span.field-validation-error>span"), TimeSpan.FromSeconds(1), true);
                Assert.AreEqual(Resources.Validation_Duplicate_Name, validation.Text);

                var input = app.Browser.FindElementByCssSelector("input#Name");
                Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
            }

            [TestMethod, TestCategory(TestCategories.IntegrationTest)]
            public void TeamNameEnsureDuplicationDetectionStillAllowsEditOtherProperties()
            {
                var ids = ITH.CreateTeams().ToList();
                var id1 = ids[0];
                app.NavigateTo<TeamController>(c => c.Edit(id1.Id));
                app.FindFormFor<TeamEditModel>()
                    .Field(f => f.Description).SetValueTo("somename")
                    .Submit();

                app.NavigateTo<TeamController>(c => c.Edit(id1.Id)); // force refresh
                app.FindFormFor<TeamEditModel>()
                    .Field(f => f.Description).ValueShouldEqual("somename");
            }
        }
    }
}
