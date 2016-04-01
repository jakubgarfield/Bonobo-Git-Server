using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpecsFor.Mvc;
using Bonobo.Git.Server.Controllers;
using System.Threading;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;

namespace Bonobo.Git.Server.Test.IntegrationTests
{
    [TestClass]
    public class MiscTests : IntegrationTestBase
    {
        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void EnsureCookiePersistBetweenBrowserRestart()
        {
            app.NavigateTo<HomeController>(c => c.LogOff()); // in case the cookie is set
            app.NavigateTo<RepositoryController>(c => c.Index(null, null));
            app.Browser.Manage().Cookies.DeleteAllCookies();
            Thread.Sleep(TimeSpan.FromSeconds(5)); // give it some time to delete the cookies

            app.NavigateTo<RepositoryController>(c => c.Index(null, null));
            app.UrlShouldMapTo<HomeController>(c => c.LogOn("/Repository/Index"));
            var form = app.FindFormFor<LogOnModel>();
            var chkField = form.Field(f => f.Username).SetValueTo("admin")
                .Field(f=> f.Password).SetValueTo("admin")
                .Field(f => f.RememberMe).Field;
            ITH.SetCheckbox(chkField, true);
            form.Submit();
            app.UrlShouldMapTo<RepositoryController>(c => c.Index(null, null));

            MvcWebApp.Driver.Shutdown();
            app = new MvcWebApp();

            app.NavigateTo<RepositoryController>(c => c.Index(null, null));
            app.UrlShouldMapTo<RepositoryController>(c => c.Index(null, null));
            // ok we re logged in with success.

            // Now let's make sure we can unset remember me
            app.NavigateTo<HomeController>(c => c.LogOff());
            app.NavigateTo<HomeController>(c => c.LogOn(""));
            form = app.FindFormFor<LogOnModel>();
            chkField = form.Field(f => f.Username).SetValueTo("admin")
                .Field(f=> f.Password).SetValueTo("admin")
                .Field(f => f.RememberMe).Field;
            ITH.SetCheckbox(chkField, false);
            form.Submit();

            app.UrlShouldMapTo<RepositoryController>(c => c.Index(null, null));

            MvcWebApp.Driver.Shutdown();
            app = new MvcWebApp();
            ITH = new IntegrationTestHelpers(app);

            app.NavigateTo<RepositoryController>(c => c.Index(null, null));
            app.UrlShouldMapTo<HomeController>(c => c.LogOn("/Repository/Index"));
        }
    }
}
