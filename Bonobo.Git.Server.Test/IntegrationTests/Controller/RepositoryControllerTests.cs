using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using SpecsFor.Mvc;

using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;


namespace Bonobo.Git.Server
{
    public static class UserExtensions
    {

        // http://stackoverflow.com/questions/915745/thoughts-on-foreach-with-enumerable-range-vs-traditional-for-loop
        public static IEnumerable<int> To(this int from, int to)
        {
            if (from < to)
            {
                while (from <= to)
                {
                    yield return from++;
                }
            }
            else
            {
                while (from >= to)
                {
                    yield return from--;
                }
            }
        }

        public static IEnumerable<T> Step<T>(this IEnumerable<T> source, int step)
        {
            if (step == 0)
            {
                throw new ArgumentOutOfRangeException("step", "Param cannot be zero.");
            }

            return source.Where((x, i) => (i % step) == 0);
        }
    }
}


namespace Bonobo.Git.Server.Test.IntegrationTests.Controller
{
    [TestClass]
    public class RepositoryControllerTests
    {
        private static MvcWebApp app;

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            //arrange
            app = new MvcWebApp();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            app.Browser.Close();
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void EnsureCheckboxesStayCheckOnCreateError()
        {
            CreateUsers(1);
            app.NavigateTo<RepositoryController>(c => c.Create());
            var form = app.FindFormFor<RepositoryDetailModel>();
            var chkboxes = form.WebApp.Browser.FindElementsByCssSelector("form.pure-form>fieldset>div.pure-control-group.checkboxlist>input");
            foreach (var chk in chkboxes)
            {
                if (!chk.Selected)
                {
                    chk.Click();
                }
            }
            form.Submit();


            form = app.FindFormFor<RepositoryDetailModel>();
            chkboxes = form.WebApp.Browser.FindElementsByCssSelector("form.pure-form>fieldset>div.pure-control-group.checkboxlist>input");
            foreach (var chk in chkboxes)
            {
                Assert.AreEqual(true, chk.Selected, "A message box was unselected eventhough we selected all!");
            }
            
        }

        public IEnumerable<Guid> CreateUsers(int count = 1, int start = 0){
            var guids = new List<Guid>();
            foreach (int i in start.To(start + count - 1))
            {
                var index = i.ToString();
                app.NavigateTo<AccountController>(c => c.Create());
                app.FindFormFor<UserCreateModel>()
                    .Field(f => f.Username).SetValueTo("TestUser" + index)
                    .Field(f => f.Name).SetValueTo("Uname" + index)
                    .Field(f => f.Surname).SetValueTo("Surname" + index)
                    .Field(f => f.Email).SetValueTo("mail" + index + "@domain.com")
                    .Field(f => f.Password).SetValueTo("aaa")
                    .Field(f => f.ConfirmPassword).SetValueTo("aaa")
                    .Submit();
                var item = app.Browser.FindElementByXPath("//div[@class='summary-success']/p");
                string id = item.GetAttribute("id");
                guids.Add(new Guid(id));
            }
            return guids;
        }

    }
}

