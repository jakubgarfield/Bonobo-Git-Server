using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using SpecsFor.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;


namespace Bonobo.Git.Server.Test.IntegrationTests.Helpers
{
    public class IntegrationTestHelpers
    {
        private readonly MvcWebApp _app;
        private readonly LoadedConfig _lc;

        public IntegrationTestHelpers(MvcWebApp app, LoadedConfig cc)
        {
            _app = app;
            _lc = cc;
        }

        public void LoginAsAdmin()
        {
            var cred = _lc.getCredentials("admin");
            _app.NavigateTo<HomeController>(c => c.LogOn("/Account"));
            _app.FindFormFor<LogOnModel>()
                .Field(f => f.Username).SetValueTo(cred.Item1)
                .Field(f => f.Password).SetValueTo(cred.Item2)
                .Submit();
            _app.UrlMapsTo<AccountController>(c => c.Index());
        }

        public void LoginAndResetDatabase()
        {
            var cred = _lc.getCredentials("admin");
            _app.NavigateTo<HomeController>(c => c.LogOnWithResetOption("/Account"));
            _app.FindFormFor<LogOnModel>()
                .Field(f => f.Username).SetValueTo(cred.Item1)
                .Field(f => f.Password).SetValueTo(cred.Item2)
                .Field(f => f.DatabaseResetCode).SetValueTo("1")
                .Submit();
            _app.UrlMapsTo<AccountController>(c => c.Index());

            // Remove leftover repositories
            var repoRoot = Path.Combine(AssemblyStartup.WebApplicationDirectory, @"App_Data\Repositories");
            foreach (var folder in Directory.GetDirectories(repoRoot))
            {
                DeleteDirectory(folder);
            }
        }

        public void LoginAsUser(UserModel user, string password = "aaa")
        {
            _app.NavigateTo<HomeController>(c => c.LogOn("/Account"));
            _app.FindFormFor<LogOnModel>()
                .Field(f => f.Username).SetValueTo(user.Username)
                .Field(f => f.Password).SetValueTo(password)
                .Submit();
            _app.UrlMapsTo<AccountController>(c => c.Index());
        }

        public Guid FindRepository(string name)
        {
            // ensure it appears on the listing
            _app.NavigateTo<RepositoryController>(c => c.Index(null, null));

            var repo_links = _app.Browser.FindElementsByCssSelector("table.repositories a.RepositoryName");
            foreach (var item in repo_links)
            {
                Debug.Print("Found repo name '{0}'", item.Text);
                if (item.Text == name)
                {
                    return new Guid(item.GetAttribute("id").Substring(5));
                }
            }
            return Guid.Empty;
        }

        public Guid CreateRepositoryOnWebInterface(string name)
        {
            _app.NavigateTo<RepositoryController>(c => c.Create());
            _app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(name)
                .Submit();
            AssertThatNoValidationErrorOccurred();
            Guid repoId = FindRepository(name);

            Assert.IsTrue(repoId != Guid.Empty, string.Format("Repository {0} not found in Index after creation!", name));
            return repoId;
        }

        public void DeleteUser(Guid userId)
        {
            _app.NavigateTo<AccountController>(c => c.Delete(userId));
            _app.FindFormFor<UserModel>().Submit();
            _app.WaitForElementToBeVisible(By.CssSelector("div.summary-success"), TimeSpan.FromSeconds(1));
            _app.UrlShouldMapTo<AccountController>(c => c.Index());
        }

        public IEnumerable<UserModel> CreateUsers(int count = 1, int start = 0, [CallerMemberName] string baseuname = "")
        {
            baseuname = MakeName(baseuname);
            var users = new List<UserModel>();
            foreach (int i in start.To(start + count - 1))
            {
                var index = i.ToString();
                var user = new UserModel
                {
                    Username = baseuname + index,
                    GivenName = "GivenName" + index,
                    Surname = "Surname" + index,
                    Email = index + "mail@domain.com"
                };
                _app.NavigateTo<AccountController>(c => c.Create());
                _app.FindFormFor<UserCreateModel>()
                    .Field(f => f.Username).SetValueTo(user.Username)
                    .Field(f => f.Name).SetValueTo(user.GivenName)
                    .Field(f => f.Surname).SetValueTo(user.Surname)
                    .Field(f => f.Email).SetValueTo(user.Email)
                    .Field(f => f.Password).SetValueTo("aaa")
                    .Field(f => f.ConfirmPassword).SetValueTo("aaa")
                    .Submit();
                AssertThatNoValidationErrorOccurred();
                var item = _app.WaitForElementToBeVisible(By.XPath("//div[@class='summary-success']/p"), TimeSpan.FromSeconds(1));
                _app.UrlShouldMapTo<AccountController>(c => c.Index());
                user.Id = new Guid(item.GetAttribute("id"));
                users.Add(user);
            }
            return users;
        }

        public static void DeleteTeam(MvcWebApp app, Guid Id)
        {
            app.NavigateTo<TeamController>(c => c.Delete(Id));
            app.FindFormFor<TeamEditModel>().Submit();
            app.WaitForElementToBeVisible(By.CssSelector("div.summary-success"), TimeSpan.FromSeconds(1));
            app.UrlShouldMapTo<TeamController>(c => c.Index());
        }

        public IEnumerable<TeamModel> CreateTeams(int count = 1, int start = 0, [CallerMemberName] string baseTeamname = "")
        {
            baseTeamname = MakeName(baseTeamname);
            var testteams = new List<TeamModel>();
            foreach (int i in start.To(start + count - 1))
            {
                var team = new TeamModel {Name = baseTeamname + i, Description = "Some team " + i};
                _app.NavigateTo<TeamController>(c => c.Create());
                _app.FindFormFor<TeamEditModel>()
                    .Field(f => f.Name).SetValueTo(team.Name)
                    .Field(f => f.Description).SetValueTo(team.Description)
                    .Submit();
                AssertThatNoValidationErrorOccurred();
                var item = _app.WaitForElementToBeVisible(By.XPath("//div[@class='summary-success']/p"), TimeSpan.FromSeconds(1));
                _app.UrlShouldMapTo<TeamController>(c => c.Index());
                team.Id = new Guid(item.GetAttribute("id"));
                testteams.Add(team);
            }
            return testteams;
        }

        public void DeleteRepositoryUsingWebsite(Guid guid)
        {
            _app.NavigateTo<RepositoryController>(c => c.Delete(guid));
            _app.FindFormFor<RepositoryDetailModel>().Submit();

            _app.WaitForElementToBeVisible(By.CssSelector("div.summary-success"), TimeSpan.FromSeconds(1));

            _app.UrlShouldMapTo<RepositoryController>(c => c.Index(null, null));

            // make sure it no longer is listed
            bool has_repo = false;
            var repo_links = _app.Browser.FindElementsByCssSelector("table.repositories a.RepositoryName");
            foreach (var item in repo_links)
            {
                if (item.GetAttribute("id") == "repo_" + guid.ToString())
                {
                    has_repo = true;
                }
            }
            Assert.AreEqual(false, has_repo, string.Format("Repository {0} still in Index after deleting!", guid));
        }

        public void SetCheckbox(IWebElement field, bool select)
        {
            if (select != field.Selected)
            {
                field.Click();
            }
        }

        public void SetCheckboxes(IEnumerable<IWebElement> fields, bool select)
        {
            foreach (var field in fields)
            {
                SetCheckbox(field, select);
            }
        }

        public void AssertThatNoValidationErrorOccurred(int timeoutSeconds = 1)
        {
            IWebElement validationSummary;
            try
            {
                // There may be a delay in the summary appearing, if there's client-side web-based validation
                validationSummary = _app.WaitForElementToBeVisible(MvcWebApp.ElementConventions.FindValidationSummary(), TimeSpan.FromSeconds(timeoutSeconds), true);
            }
            catch (WebDriverTimeoutException)
            {
                // This means that there was no validation summary on the page, which means the form was OK
                return;
            }
            var summaryText = validationSummary != null ? validationSummary.Text : "No text available";
            Assert.Fail("Form submission error occurred, ValidationSummary " + summaryText);
        }

        public void AssertThatValidationErrorContains(string matchText, int timeoutSeconds = 1)
        {
            var validationSummary = _app.WaitForElementToBeVisible(MvcWebApp.ElementConventions.FindValidationSummary(), TimeSpan.FromSeconds(timeoutSeconds));
            var summaryText = validationSummary.Text;
            if (!summaryText.Contains(matchText))
            {
                Assert.Fail("Form submission validation error should have contained '{0}' but was '{1}'", matchText, summaryText);
            }
        }

        public void DeleteDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return;

            // We have to tolerate intermittent errors during directory deletion, because
            // other parts of Windows sometimes hold locks on files briefly
            // Multiple tries normally fixes it
            for (int attempt = 10; attempt >= 0; attempt--)
            {
                try
                {
                    var directory = new DirectoryInfo(directoryPath) { Attributes = FileAttributes.Normal };
                    foreach (var item in directory.GetFiles("*.*", SearchOption.AllDirectories))
                    {
                        item.Attributes = FileAttributes.Normal;
                    }
                    directory.Delete(true);
                    return;
                }
                catch
                {
                    if (attempt == 0)
                    {
                        throw;
                    }
                    Thread.Sleep(1000);
                }
            }
        }

        /* The default is to use the default calling methods name */
        public static string MakeName([CallerMemberName] string name = "", int maxLen = 50)
        {
            // Prefer beginning + end from user as this make it possible to use
            // Curname + extension as uniqueness
            if (name.Length > maxLen)
            {
                int partLen = (maxLen / 2) - 4;
                name = name.Substring(0, partLen) + "..." + name.Substring(name.Length - partLen, partLen);
            }
            return name;
        }

        public void SetGlobalSetting<T>(Expression<Func<GlobalSettingsModel, T>> optionExpression, string value)
        {
            _app.NavigateTo<SettingsController>(c => c.Index());
            var form = _app.FindFormFor<GlobalSettingsModel>();
            var field = form.Field(optionExpression);
            field.SetValueTo(value);
            form.Submit();
            AssertSuccessMessageIsDisplayed();
        }

        public void SetGlobalSetting<T>(Expression<Func<GlobalSettingsModel, T>> optionExpression, bool value)
        {
            _app.NavigateTo<SettingsController>(c => c.Index());
            var form = _app.FindFormFor<GlobalSettingsModel>();
            var field = form.Field(optionExpression);
            SetCheckbox(field.Field, (bool)value);
            form.Submit();
            AssertSuccessMessageIsDisplayed();
        }

        public static void SetElementAttribute(IWebElement element, string attName, string attValue)
        {
            MvcWebApp.Driver.GetDriver()
                .ExecuteScript("arguments[0].setAttribute(arguments[1], arguments[2]);", 
                element, attName, attValue);
        }

        public void AssertSuccessMessageIsDisplayed(int timeoutSeconds = 1)
        {
            _app.WaitForElementToBeVisible(By.CssSelector("div.summary-success"), TimeSpan.FromSeconds(timeoutSeconds));
        }
    }

}
