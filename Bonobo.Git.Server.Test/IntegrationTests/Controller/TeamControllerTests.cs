using System.Linq;
using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.IntegrationTests.Controller
{
    public class TeamControllerSpecs
    {
        [TestClass]
        public class TeamControllerTests : IntegrationTestBase
        {
            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void TeamNameEnsureDuplicationDetectionAsYouTypeWorksOnCreation()
            {
                var id1 = ITH.CreateTeams(1).Single();
                app.NavigateTo<TeamController>(c => c.Create());
                app.FindFormFor<TeamEditModel>()
                    .Field(f => f.Name).SetValueTo(id1.Name)
                    .Field(f => f.Description).Click(); // Set focus

                var input = app.Browser.FindElementByCssSelector("input#Name");
                Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
            }

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void TeamNameEnsureDuplicationDetectionAsYouTypeWorksOnEdit()
            {
                var teams = ITH.CreateTeams(2).ToList();
                var id1 = teams[0];
                var id2 = teams[1].Id;
                    app.NavigateTo<TeamController>(c => c.Edit(id2));
                    app.FindFormFor<TeamEditModel>()
                        .Field(f => f.Name).SetValueTo(id1.Name)
                        .Field(f => f.Description).Click(); // Set focus


                    var input = app.Browser.FindElementByCssSelector("input#Name");
                    Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
            }

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void TeamNameEnsureDuplicationDetectionStillAllowsEditOtherProperties()
            {
                var ids = ITH.CreateTeams(1).ToList();
                var id1 = ids[0].Id;
                app.NavigateTo<TeamController>(c => c.Edit(id1));
                app.FindFormFor<TeamEditModel>()
                    .Field(f => f.Description).SetValueTo("somename")
                    .Submit();

                app.NavigateTo<TeamController>(c => c.Edit(id1)); // force refresh
                app.FindFormFor<TeamEditModel>()
                    .Field(f => f.Description).ValueShouldEqual("somename");
            }
        }
    }
}
