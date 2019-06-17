using System;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Bonobo.Git.Server.Controllers
{
    public class SettingsController : Controller
    {
        private readonly IHostingEnvironment hostingEnvironment;

        public SettingsController(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        [Authorize(Policy = "Web", Roles = Definitions.Roles.Administrator)]
        public ActionResult Index()
        {
            return View(new GlobalSettingsModel
            {
                AllowAnonymousPush = UserConfiguration.Current.AllowAnonymousPush,
                RepositoryPath = UserConfiguration.Current.RepositoryPath,
                AllowAnonymousRegistration = UserConfiguration.Current.AllowAnonymousRegistration,
                AllowUserRepositoryCreation = UserConfiguration.Current.AllowUserRepositoryCreation,
                AllowPushToCreate = UserConfiguration.Current.AllowPushToCreate,
                DefaultLanguage = UserConfiguration.Current.DefaultLanguage,
                SiteTitle = UserConfiguration.Current.SiteTitle,
                SiteLogoUrl = UserConfiguration.Current.SiteLogoUrl,
                SiteFooterMessage = UserConfiguration.Current.SiteFooterMessage,
                SiteCssUrl = UserConfiguration.Current.SiteCssUrl,
                IsCommitAuthorAvatarVisible = UserConfiguration.Current.IsCommitAuthorAvatarVisible,
                LinksRegex = UserConfiguration.Current.LinksRegex,
                LinksUrl = UserConfiguration.Current.LinksUrl,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Web", Roles = Definitions.Roles.Administrator)]
        public ActionResult Index(GlobalSettingsModel model)
        {
            if (AuthenticationSettings.DemoModeActive)
            {
                return RedirectToAction("Unauthorized", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (Directory.Exists(Path.IsPathRooted(model.RepositoryPath)
                                         ? model.RepositoryPath
                                         : Path.Combine(hostingEnvironment.ContentRootPath, model.RepositoryPath)))
                    {
                        UserConfiguration.Current.AllowAnonymousPush = model.AllowAnonymousPush;
                        UserConfiguration.Current.RepositoryPath = model.RepositoryPath;
                        UserConfiguration.Current.AllowAnonymousRegistration = model.AllowAnonymousRegistration;
                        UserConfiguration.Current.AllowUserRepositoryCreation = model.AllowUserRepositoryCreation;
                        UserConfiguration.Current.AllowPushToCreate = model.AllowPushToCreate;
                        UserConfiguration.Current.DefaultLanguage = model.DefaultLanguage;
                        UserConfiguration.Current.SiteTitle = model.SiteTitle;
                        UserConfiguration.Current.SiteLogoUrl = model.SiteLogoUrl;
                        UserConfiguration.Current.SiteFooterMessage = model.SiteFooterMessage;
                        UserConfiguration.Current.SiteCssUrl = model.SiteCssUrl;
                        UserConfiguration.Current.IsCommitAuthorAvatarVisible = model.IsCommitAuthorAvatarVisible;
                        UserConfiguration.Current.LinksRegex = model.LinksRegex;
                        UserConfiguration.Current.LinksUrl = model.LinksUrl;
                        UserConfiguration.Current.Save();

                        HttpContext.Session.SetString("Culture", JsonConvert.SerializeObject(new CultureInfo(model.DefaultLanguage)));

                        TempData["UpdateSuccess"] = true;
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("RepositoryPath", Resources.Settings_RepositoryPathNotExists);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    ModelState.AddModelError("RepositoryPath", Resources.Settings_RepositoryPathUnauthorized);
                }
            }

            return View(model);
        }
    }
}
