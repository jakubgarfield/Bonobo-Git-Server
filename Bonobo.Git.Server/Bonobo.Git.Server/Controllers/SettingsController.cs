using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bonobo.Git.Server.Models;
using System.Configuration;
using System.Web.Configuration;
using System.IO;
using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Configs;

namespace Bonobo.Git.Server.Controllers
{
    public class SettingsController : Controller
    {
        [FormsAuthorizeAttribute(Roles = Definitions.Roles.Administrator)]
        public ActionResult Index()
        {            
            return View(new GlobalSettingsModel
            {
                AllowAnonymousPush = UserConfiguration.Current.AllowAnonymousPush,
                RepositoryPath = UserConfiguration.Current.Repositories,
                AllowAnonymousRegistration = UserConfiguration.Current.AllowAnonymousRegistration,
                AllowUserRepositoryCreation = UserConfiguration.Current.AllowUserRepositoryCreation,
            });
        }

        [HttpPost]
        [FormsAuthorizeAttribute(Roles = Definitions.Roles.Administrator)]
        public ActionResult Index(GlobalSettingsModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Directory.Exists(model.RepositoryPath))
                    {
                        System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(model.RepositoryPath);

                        UserConfiguration.Current.AllowAnonymousPush = model.AllowAnonymousPush;
                        UserConfiguration.Current.Repositories = model.RepositoryPath;
                        UserConfiguration.Current.AllowAnonymousRegistration = model.AllowAnonymousRegistration;
                        UserConfiguration.Current.AllowUserRepositoryCreation = model.AllowUserRepositoryCreation;
                        UserConfiguration.Current.Save();

                        ViewBag.UpdateSuccess = true;
                    }
                    else
                    {
                        ModelState.AddModelError("RepositoryPath", Resources.Settings_RepositoryPathNotExists);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    ModelState.AddModelError("RepositoryPath", Resources.Settings_RepositoryPathNotExists);
                }
            }

            return View(model);
        }
    }
}
