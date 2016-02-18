using System;
using SpecsFor;
using SpecsFor.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.IntegrationTests
{
    public class HomeControllerSpecs
    {
        [TestClass]
        public class when_viewing_the_homepage : SpecsFor<MvcWebApp>
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

            [TestMethod]
            public void UserDetailRequiresLogin()
            {
                app.NavigateTo<HomeController>(c => c.LogOff());
                app.UrlShouldMapTo<HomeController>(c => c.LogOn("/Home/Index"));

                app.NavigateTo<AccountController>(c => c.Detail("123"));

                app.UrlShouldMapTo<HomeController>(c => c.LogOn("/Account/123"));
            }

            [TestMethod]
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
