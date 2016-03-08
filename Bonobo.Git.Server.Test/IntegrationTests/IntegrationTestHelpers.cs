using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using SpecsFor.Mvc;

using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.IntegrationTests;
using Bonobo.Git.Server.Test.MembershipTests.ADTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

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


namespace Bonobo.Git.Server.Test.IntegrationTests.Helpers
{
    public class IntegrationTestHelpers
    {
        private readonly MvcWebApp _app;

        public IntegrationTestHelpers(MvcWebApp app)
        {
            _app = app;
        }

        public void Login()
        {
            _app.NavigateTo<HomeController>(c => c.LogOn("/Account"));
            _app.FindFormFor<LogOnModel>()
                .Field(f => f.Username).SetValueTo("admin")
                .Field(f => f.Password).SetValueTo("admin")
                .Submit();
            _app.UrlMapsTo<AccountController>(c => c.Index());
        }

        public void LoginAndResetDatabase()
        {
            _app.NavigateTo<HomeController>(c => c.LogOnWithResetOption("/Account"));
            _app.FindFormFor<LogOnModel>()
                .Field(f => f.Username).SetValueTo("admin")
                .Field(f => f.Password).SetValueTo("admin")
                .Field(f => f.DatabaseResetCode).SetValueTo("1")
                .Submit();
            _app.UrlMapsTo<AccountController>(c => c.Index());

            // Remote an repo directories
            var repoRoot = Path.Combine(AssemblyStartup.WebApplicationDirectory, @"app_data\Repositories");
            foreach (var folder in Directory.GetDirectories(repoRoot))
            {
                DeleteDirectory(folder);
            }
        }

        public void LoginAsNumberedUser(int index)
        {
            _app.NavigateTo<HomeController>(c => c.LogOn("/Account"));
            _app.FindFormFor<LogOnModel>()
                .Field(f => f.Username).SetValueTo("TestUser"+index)
                .Field(f => f.Password).SetValueTo("aaa")
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
        }

        public IEnumerable<Guid> CreateUsers(int count = 1, int start = 0)
        {
            var guids = new List<Guid>();
            foreach (int i in start.To(start + count - 1))
            {
                var index = i.ToString();
                _app.NavigateTo<AccountController>(c => c.Create());
                _app.FindFormFor<UserCreateModel>()
                    .Field(f => f.Username).SetValueTo("TestUser" + index)
                    .Field(f => f.Name).SetValueTo("Uname" + index)
                    .Field(f => f.Surname).SetValueTo("Surname" + index)
                    .Field(f => f.Email).SetValueTo("mail" + index + "@domain.com")
                    .Field(f => f.Password).SetValueTo("aaa")
                    .Field(f => f.ConfirmPassword).SetValueTo("aaa")
                    .Submit();
                var item = _app.Browser.FindElementByXPath("//div[@class='summary-success']/p");
                string id = item.GetAttribute("id");
                guids.Add(new Guid(id));
            }
            return guids;
        }

        public IEnumerable<TeamModel> CreateTeams(int count = 1, int start = 0)
        {
            var testteams = new List<TeamModel>();
            foreach (int i in start.To(start + count - 1))
            {
                _app.NavigateTo<TeamController>(c => c.Create());
                _app.FindFormFor<TeamEditModel>()
                    .Field(f => f.Name).SetValueTo("Team" + i)
                    .Field(f => f.Description).SetValueTo("Nice team number " + i)
                    .Submit();
                _app.UrlShouldMapTo<TeamController>(c => c.Index());
                var item = _app.Browser.FindElementByXPath("//div[@class='summary-success']/p");
                string id = item.GetAttribute("id");
                testteams.Add(new TeamModel { Id= new Guid(id), Name = "Team" + i});
            }
            return testteams;
        }

        public void DeleteRepositoryUsingWebsite(Guid guid)
        {
            _app.NavigateTo<RepositoryController>(c => c.Delete(guid));
            _app.FindFormFor<RepositoryDetailModel>().Submit();

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

        public void AssertThatNoValidationErrorOccurred()
        {
            // There may be a delay in the summary appearing, if there's client-side web-based validation
            Thread.Sleep(1000);

            IWebElement validationSummary;
            try
            {
                validationSummary = _app.ValidationSummary;
            }
            catch (NoSuchElementException)
            {
                // This means that there was no validation summary on the page, which means the form was OK
                return;
            }
            var summaryText = validationSummary != null ? validationSummary.Text : "No text available";
            Assert.Fail("Form submission error occurred, ValidationSummary " + summaryText);
        }

        public void AssertThatValidationErrorContains(string matchText)
        {
            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    var summaryText = _app.ValidationSummary.Text;
                    if (!summaryText.Contains(matchText))
                    {
                        Assert.Fail("Form submission validation error should have contained '{0}' but was '{1}'",
                            matchText, summaryText);
                    }
                    return;
                }
                catch (NoSuchElementException)
                {
                    Debug.Print("Retrying ValidationSummary check");
                    Thread.Sleep(1000);
                }
            }
            Assert.Fail("No validation summary found on page");
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

        public string MakeRepoName(MethodBase currentMethod)
        {
            var methodName = currentMethod.Name;
            return methodName.Substring(0, Math.Min(40, methodName.Length));
        }
    }
}
