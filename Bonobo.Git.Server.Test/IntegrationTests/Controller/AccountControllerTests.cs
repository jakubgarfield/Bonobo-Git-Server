using System;
using SpecsFor.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;

namespace Bonobo.Git.Server.Test.Integration.Web
{
    public class HomeControllerSpecs
    {
        [TestClass]
        public class AccountControllerTests
        {
            private static MvcWebApp app;

            [ClassInitialize]
            public static void MyClassInitialize(TestContext testContext)
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
            public void UserDetailRequiresLogin()
            {
                app.NavigateTo<HomeController>(c => c.LogOff());
                app.UrlShouldMapTo<HomeController>(c => c.LogOn("/Home/Index"));

                app.NavigateTo<AccountController>(c => c.Detail(new Guid("7479fc09-2c0b-4e93-a2cf-5e4bbf6bab4f")));

                app.UrlShouldMapTo<HomeController>(c => c.LogOn("/Account/Detail/7479fc09-2c0b-4e93-a2cf-5e4bbf6bab4f"));
            }

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
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
        }
    }
}
