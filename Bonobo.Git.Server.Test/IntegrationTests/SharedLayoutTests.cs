using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Test.Integration.Web;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;
using SpecsFor.Mvc;

namespace Bonobo.Git.Server.Test.IntegrationTests
{
    using OpenQA.Selenium.Support.UI;
    using OpenQA.Selenium;
    using System.Threading;
    using ITH = IntegrationTestHelpers;

    [TestClass]
    public class SharedLayoutTests : IntegrationTestBase
    {
        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void DropdownNavigationWorks()
        {
            var reponame = ITH.MakeName();
            var otherreponame = ITH.MakeName(reponame + "_other");
            var repoId = ITH.CreateRepositoryOnWebInterface(reponame);
            var otherrepoId = ITH.CreateRepositoryOnWebInterface(otherreponame);

            app.NavigateTo<RepositoryController>(c => c.Detail(otherrepoId));

            var element = app.Browser.FindElementByCssSelector("select#Repositories");
            var dropdown = new SelectElement(element);
            dropdown.SelectByText(reponame);
            Thread.Sleep(2000);

            app.UrlMapsTo<RepositoryController>(c => c.Detail(repoId));

            app.WaitForElementToBeVisible(By.CssSelector("select#Repositories"), TimeSpan.FromSeconds(10));
            dropdown = new SelectElement(app.Browser.FindElementByCssSelector("select#Repositories"));
            dropdown.SelectByText(otherreponame);
            Thread.Sleep(2000);

            app.UrlMapsTo<RepositoryController>(c => c.Detail(otherrepoId));
        }

    }
}
