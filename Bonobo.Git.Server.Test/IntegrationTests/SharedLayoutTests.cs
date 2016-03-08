using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;
using SpecsFor.Mvc;

namespace Bonobo.Git.Server.Test.IntegrationTests
{
    using ITH = IntegrationTestHelpers;
    using OpenQA.Selenium.Support.UI;
    using OpenQA.Selenium;
    using System.Reflection;

    [TestClass]
    public class SharedLayoutTests
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
        public void DropdownNavigationWorks()
        {
            var reponame = MethodBase.GetCurrentMethod().Name;
            using (var id1 = ITH.CreateRepositoryOnWebInterface(app, reponame))
            using (var id2 = ITH.CreateRepositoryOnWebInterface(app, reponame + "_other"))
            {

                app.NavigateTo<RepositoryController>(c => c.Detail(id2));

                var element = app.Browser.FindElementByCssSelector("select#Repositories");
                var dropdown = new SelectElement(element);
                dropdown.SelectByText(id1.Name);

                app.UrlMapsTo<RepositoryController>(c => c.Detail(id1));


                app.WaitForElementToBeVisible(By.CssSelector("select#Repositories"), TimeSpan.FromSeconds(10));
                dropdown = new SelectElement(app.Browser.FindElementByCssSelector("select#Repositories"));
                dropdown.SelectByText(id2.Name);

                app.UrlMapsTo<RepositoryController>(c => c.Detail(id2));


            }
        }
    }
}
