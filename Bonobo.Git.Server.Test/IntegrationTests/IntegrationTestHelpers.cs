using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using SpecsFor.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

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
    static class IntegrationTestHelpers
    {
        public static void Login(MvcWebApp app)
        {
            app.NavigateTo<HomeController>(c => c.LogOn("/Account"));
            app.FindFormFor<LogOnModel>()
                .Field(f => f.Username).SetValueTo("admin")
                .Field(f => f.Password).SetValueTo("admin")
                .Submit();
            app.UrlMapsTo<AccountController>(c => c.Index());
        }

        public static void LoginAsNumberedUser(MvcWebApp app, int index)
        {
            app.NavigateTo<HomeController>(c => c.LogOn("/Account"));
            app.FindFormFor<LogOnModel>()
                .Field(f => f.Username).SetValueTo("TestUser" + index)
                .Field(f => f.Password).SetValueTo("aaa")
                .Submit();
            app.UrlMapsTo<AccountController>(c => c.Index());
        }

        public static Guid FindRepository(MvcWebApp app, string name)
        {

            // ensure it appears on the listing
            app.NavigateTo<RepositoryController>(c => c.Index(null, null));

            var repo_links = app.Browser.FindElementsByCssSelector("table.repositories a.RepositoryName");
            foreach (var item in repo_links)
            {
                if (item.Text == name)
                {
                    return new Guid(item.GetAttribute("id").Substring(5));
                }
            }
            return Guid.Empty;
        }

        public static TestRepo CreateRepositoryOnWebInterface(MvcWebApp app, [CallerMemberName] string name = "", bool truncateLongName = true)
        {
            if (truncateLongName && name.Length > 50)
            {
                name = name.Substring(0, 20) + "..." + name.Substring(name.Length - 20, 20);
            }
            app.NavigateTo<RepositoryController>(c => c.Create());
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(name)
                .Submit();
            Guid repoId = FindRepository(app, name);

            Assert.IsTrue(repoId != Guid.Empty, string.Format("Repository {0} not found in Index after creation!", name));
            return new TestRepo(repoId, name, app);
        }

        public static void DeleteUser(MvcWebApp app, Guid userId)
        {
            app.NavigateTo<AccountController>(c => c.Delete(userId));
            app.FindFormFor<UserModel>().Submit();
        }

        public static IEnumerable<TestTeam> CreateTeams(MvcWebApp app, int count = 1, int start = 0)
        {
            var testteams = new List<TestTeam>();
            foreach (int i in start.To(start + count - 1))
            {
                app.NavigateTo<TeamController>(c => c.Create());
                app.FindFormFor<TeamEditModel>()
                    .Field(f => f.Name).SetValueTo("Team" + i)
                    .Field(f => f.Description).SetValueTo("Nice team number " + i)
                    .Submit();
                app.UrlShouldMapTo<TeamController>(c => c.Index());
                var item = app.Browser.FindElementByXPath("//div[@class='summary-success']/p");
                string id = item.GetAttribute("id");
                testteams.Add(new TestTeam(new Guid(id), "Team" + i, app));
            }
            return testteams;
        }

        public static void DeleteTeam(MvcWebApp app, Guid Id)
        {
            app.NavigateTo<TeamController>(c => c.Delete(Id));
            app.FindFormFor<TeamEditModel>().Submit();
        }

        public static IEnumerable<TestUser> CreateUsers(MvcWebApp app, int count = 1, int start = 0)
        {
            var testusers = new List<TestUser>();
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
                app.UrlShouldMapTo<AccountController>(c => c.Index());
                var item = app.Browser.FindElementByXPath("//div[@class='summary-success']/p");
                string id = item.GetAttribute("id");
                testusers.Add(new TestUser(new Guid(id), "TestUser" + index, app));
            }
            return testusers;
        }

        public static void DeleteRepositoryUsingWebsite(MvcWebApp app, Guid guid)
        {
            app.NavigateTo<RepositoryController>(c => c.Delete(guid));
            app.FindFormFor<RepositoryDetailModel>().Submit();

            // make sure it no longer is listed
            bool has_repo = false;
            var repo_links = app.Browser.FindElementsByCssSelector("table.repositories a.RepositoryName");
            foreach (var item in repo_links)
            {
                if (item.GetAttribute("id") == "repo_" + guid.ToString())
                {
                    has_repo = true;
                }
            }
            Assert.AreEqual(false, has_repo, string.Format("Repository {0} still in Index after deleting!", guid));
        }

        public static void SetCheckbox(IWebElement field, bool select)
        {
            if (select != field.Selected)
            {
                field.Click();
            }
        }

        public static void SetCheckboxes(IEnumerable<IWebElement> fields, bool select)
        {
            foreach (var field in fields)
            {
                SetCheckbox(field, select);
            }
        }

        public static void AssertThatNoValidationErrorOccurred(MvcWebApp app)
        {
            IWebElement validationSummary;
            try
            {
                validationSummary = app.ValidationSummary;
            }
            catch (NoSuchElementException)
            {
                // This means that there was no validation summary on the page, which means the form was OK
                return;
            }
            var summaryText = validationSummary != null ? validationSummary.Text : "No text available";
            Assert.Fail("Form submission error occurred, ValidationSummary " + summaryText);
        }

        public static void AssertThatValidationErrorContains(MvcWebApp app, string matchText)
        {
            try
            {
                var summaryText = app.ValidationSummary.Text;
                if (!summaryText.Contains(matchText))
                {
                    Assert.Fail("Form submission validation error should have contained '{0}' but was '{1}'", matchText, summaryText);
                }
            }
            catch (NoSuchElementException)
            {
                Assert.Fail("No validation summary found on page");
            }
        }

        public class TestRepo : IDisposable
        {
            public TestRepo(Guid id, string name, MvcWebApp app)
            {
                Id = id;
                this.App = app;
                this.Name = name;
            }

            public void Dispose()
            {
                Debug.Write(string.Format("Disposing repo '{0}'", Name));
                Console.Write(string.Format("Disposing repo '{0}'", Name));
                DeleteRepositoryUsingWebsite(App, Id);
            }

            public Guid Id;
            public string Name;
            public MvcWebApp App;

            public static implicit operator Guid(TestRepo wr)
            {
                return wr.Id;
            }
        }

        public class TestTeam : IDisposable
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public MvcWebApp App { get; set; }

            public TestTeam(Guid guid, string name, MvcWebApp app)
            {
                Id = guid;
                Name = name;
                this.App = app;
            }

            public void Dispose()
            {
                Debug.Write(string.Format("Disposing team '{0}'.", Name));
                Console.Write(string.Format("Disposing team '{0}'.", Name));
                DeleteTeam(App, Id);
            }

            public static implicit operator Guid(TestTeam tt)
            {
                return tt.Id;
            }
        }

        public class TestUser : IDisposable
        {
            public Guid Id;
            public string Username;
            public MvcWebApp App;

            public TestUser(Guid guid, string Username, MvcWebApp app)
            {
                Id = guid;
                this.Username = Username;
                this.App = app;
            }

            public void Dispose()
            {
                Debug.Write(string.Format("Disposing user '{0}'.", Username));
                Console.Write(string.Format("Disposing user '{0}'.", Username));
                DeleteUser(App, Id);
            }

            public static implicit operator Guid(TestUser tu)
            {
                return tu.Id;
            }
        }
    }
}
