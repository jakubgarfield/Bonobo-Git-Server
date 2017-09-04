using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpecsFor.Mvc;
using Bonobo.Git.Server.Controllers;
using System.Threading;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;

namespace Bonobo.Git.Server.Test.IntegrationTests
{
    using TC = TestCategories;
    [TestClass]
    public class MiscTests : IntegrationTestBase
    {
        [TestMethod, TestCategory(TC.IntegrationTest), TestCategory(TC.AuthForms)]
        public void EnsureCookiePersistBetweenBrowserRestart()
        {
            app.NavigateTo<HomeController>(c => c.LogOff()); // in case the cookie is set
            app.NavigateTo<RepositoryController>(c => c.Index(null, null));
            app.Browser.Manage().Cookies.DeleteAllCookies();
            Thread.Sleep(TimeSpan.FromSeconds(5)); // give it some time to delete the cookies

            app.NavigateTo<AccountController>(c => c.Detail(new Guid("7479fc09-2c0b-4e93-a2cf-5e4bbf6bab4f")));
            app.UrlShouldMapTo<HomeController>(c => c.LogOn("/Account/Detail/7479fc09-2c0b-4e93-a2cf-5e4bbf6bab4f"));

            var form = app.FindFormFor<LogOnModel>();
            var chkField = form.Field(f => f.Username).SetValueTo("admin")
                .Field(f => f.Password).SetValueTo("admin")
                .Field(f => f.RememberMe).Field;
            ITH.SetCheckbox(chkField, true);
            form.Submit();
            app.UrlShouldMapTo<AccountController>(c => c.Detail(new Guid("7479fc09-2c0b-4e93-a2cf-5e4bbf6bab4f")));
             
            MvcWebApp.Driver.Shutdown();
            app = new MvcWebApp();
            ITH = new IntegrationTestHelpers(app, lc);
            app.NavigateTo<AccountController>(c => c.Detail(new Guid("7479fc09-2c0b-4e93-a2cf-5e4bbf6bab4f")));
            app.UrlShouldMapTo<AccountController>(c => c.Detail(new Guid("7479fc09-2c0b-4e93-a2cf-5e4bbf6bab4f")));
            // ok we re logged in with success.

            // Now let's make sure we can unset remember me
            app.NavigateTo<HomeController>(c => c.LogOff());
            app.NavigateTo<HomeController>(c => c.LogOn(""));
            form = app.FindFormFor<LogOnModel>();
            chkField = form.Field(f => f.Username).SetValueTo("admin")
                .Field(f => f.Password).SetValueTo("admin")
                .Field(f => f.RememberMe).Field;
            ITH.SetCheckbox(chkField, false);
            form.Submit();

            app.UrlShouldMapTo<RepositoryController>(c => c.Index(null, null));

            MvcWebApp.Driver.Shutdown();
            app = new MvcWebApp();
            ITH = new IntegrationTestHelpers(app, lc);

            app.NavigateTo<AccountController>(c => c.Detail(new Guid("7479fc09-2c0b-4e93-a2cf-5e4bbf6bab4f")));
            app.UrlShouldMapTo<HomeController>(c => c.LogOn("/Account/Detail/7479fc09-2c0b-4e93-a2cf-5e4bbf6bab4f"));
        }
    }
}
