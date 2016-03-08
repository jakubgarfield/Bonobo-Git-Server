using System;
using System.Linq;
using SpecsFor.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;

namespace Bonobo.Git.Server.Test.Integration.Web
{
    using ITH = IntegrationTestHelpers;
    using System.Collections.Generic;
    public class HomeControllerSpecs
    {
        [TestClass]
        public class TeamControllerTests
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
            public void InitTest()
            {
                IntegrationTestHelpers.Login(app);
            }

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void TeamNameEnsureDuplicationDetectionAsYouTypeWorksOnCreation()
            {
                using (var id1 = ITH.CreateTeams(app, 1).Single())
                {
                    app.NavigateTo<TeamController>(c => c.Create());
                    var form = app.FindFormFor<TeamEditModel>()
                        .Field(f => f.Name).SetValueTo(id1.Name)
                        .Field(f => f.Description).Click(); // Set focus


                    var input = app.Browser.FindElementByCssSelector("input#Name");
                    Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
                }
            }

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void TeamNameEnsureDuplicationDetectionAsYouTypeWorksOnEdit()
            {
                var ids = ITH.CreateTeams(app, 2).ToList();
                using (var id1 = ids[0])
                using (var id2 = ids[1])
                {
                    app.NavigateTo<TeamController>(c => c.Edit(id2));
                    var form = app.FindFormFor<TeamEditModel>()
                        .Field(f => f.Name).SetValueTo(id1.Name)
                        .Field(f => f.Description).Click(); // Set focus


                    var input = app.Browser.FindElementByCssSelector("input#Name");
                    Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));

                }
                ids.Clear();
            }

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void TeamNameEnsureDuplicationDetectionStillAllowsEditOtherProperties()
            {
                var ids = ITH.CreateTeams(app, 1).ToList();
                using(var id1 = ids[0])
                {
                    app.NavigateTo<TeamController>(c => c.Edit(id1));
                    app.FindFormFor<TeamEditModel>()
                        .Field(f => f.Description).SetValueTo("somename")
                        .Submit();

                    app.NavigateTo<TeamController>(c => c.Edit(id1)); // force refresh
                    app.FindFormFor<TeamEditModel>()
                        .Field(f => f.Description).ValueShouldEqual("somename");
                }
                ids.Clear();
            }
        }
    }
}
