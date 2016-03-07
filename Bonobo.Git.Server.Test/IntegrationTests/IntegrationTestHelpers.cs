using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SpecsFor.Mvc;

using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Controllers;
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
        

        public Guid CreateRepositoryOnWebInterface(string name)
        {
            app.NavigateTo<RepositoryController>(c => c.Create());
            app.FindFormFor<RepositoryDetailModel>()

            // ensure it appears on the listing
            _app.NavigateTo<RepositoryController>(c => c.Index(null, null));

            var repo_links = _app.Browser.FindElementsByCssSelector("table.repositories a.RepositoryName");
            foreach (var item in repo_links)
            {
                if (item.Text == name)
                {
                    return new Guid(item.GetAttribute("id").Substring(5));
                }
            }
            return Guid.Empty;
        }

        public Guid CreateRepositoryOnWebInterface(string name)
        {
            app.NavigateTo<RepositoryController>(c => c.Create());
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(name)
                .Submit();
            Guid repoId = FindRepository(app, name);

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
            try
            {
                var summaryText = _app.ValidationSummary.Text;
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

    }
}
