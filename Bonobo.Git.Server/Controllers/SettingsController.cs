﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Controllers
{
    public class SettingsController : Controller
    {
        [WebAuthorizeAttribute(Roles = Definitions.Roles.Administrator)]
        public ActionResult Index()
        {
            return View(new GlobalSettingsModel
            {
                AllowAnonymousPush = UserConfiguration.Current.AllowAnonymousPush,
                RepositoryPath = UserConfiguration.Current.Repositories,
                AllowAnonymousRegistration = UserConfiguration.Current.AllowAnonymousRegistration,
                AllowAnonymousListing = UserConfiguration.Current.AllowAnonymousListing,
                AllowUserRepositoryCreation = UserConfiguration.Current.AllowUserRepositoryCreation,
                DefaultLanguage = UserConfiguration.Current.DefaultLanguage,
                SiteTitle = UserConfiguration.Current.SiteTitle,
                SiteLogoUrl = UserConfiguration.Current.SiteLogoUrl,
                SiteFooterMessage = UserConfiguration.Current.SiteFooterMessage,
                IsCommitAuthorAvatarVisible = UserConfiguration.Current.IsCommitAuthorAvatarVisible,                
            });
        }

        [HttpPost]
        [WebAuthorizeAttribute(Roles = Definitions.Roles.Administrator)]
        public ActionResult Index(GlobalSettingsModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Directory.Exists(model.RepositoryPath))
                    {
                        UserConfiguration.Current.AllowAnonymousPush = model.AllowAnonymousPush;
                        UserConfiguration.Current.Repositories = model.RepositoryPath;
                        UserConfiguration.Current.AllowAnonymousRegistration = model.AllowAnonymousRegistration;
                        UserConfiguration.Current.AllowUserRepositoryCreation = model.AllowUserRepositoryCreation;
                        UserConfiguration.Current.AllowAnonymousListing = model.AllowAnonymousListing;
                        UserConfiguration.Current.DefaultLanguage = model.DefaultLanguage;
                        UserConfiguration.Current.SiteTitle = model.SiteTitle;
                        UserConfiguration.Current.SiteLogoUrl = model.SiteLogoUrl;
                        UserConfiguration.Current.SiteFooterMessage = model.SiteFooterMessage;
                        UserConfiguration.Current.IsCommitAuthorAvatarVisible = model.IsCommitAuthorAvatarVisible;
                        UserConfiguration.Current.Save();

                        this.Session["Culture"] = new CultureInfo(model.DefaultLanguage);

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
