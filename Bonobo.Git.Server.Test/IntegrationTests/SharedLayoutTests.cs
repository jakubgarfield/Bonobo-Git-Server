using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bonobo.Git.Server.Controllers;

namespace Bonobo.Git.Server.Test.IntegrationTests
{
    using OpenQA.Selenium.Support.UI;
    using OpenQA.Selenium;

    [TestClass]
    public class SharedLayoutTests : IntegrationTestBase
    {
        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void DropdownNavigationWorks()
        {
            var reponame = "A_Nice_Repo";
            var id1 = ITH.CreateRepositoryOnWebInterface(reponame);
            var id2 = ITH.CreateRepositoryOnWebInterface("other_name");

            app.NavigateTo<RepositoryController>(c => c.Detail(id2));

            var element = app.Browser.FindElementByCssSelector("select#Repositories");
            var dropdown = new SelectElement(element);
            dropdown.SelectByText(reponame);

            app.UrlMapsTo<RepositoryController>(c => c.Detail(id1));


            app.WaitForElementToBeVisible(By.CssSelector("select#Repositories"), TimeSpan.FromSeconds(10));
            dropdown = new SelectElement(app.Browser.FindElementByCssSelector("select#Repositories"));
            dropdown.SelectByText("other_name");

            app.UrlMapsTo<RepositoryController>(c => c.Detail(id2));
        }

    }
}
