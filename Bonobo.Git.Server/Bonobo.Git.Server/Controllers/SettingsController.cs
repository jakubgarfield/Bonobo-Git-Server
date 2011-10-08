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

namespace Bonobo.Git.Server.Controllers
{
    public class SettingsController : Controller
    {
        [AuthorizeRedirect(Roles = Definitions.Roles.Administrator)]
        public ActionResult Index()
        {            
            return View(new GlobalSettingsModel
            {              
                AllowAnonymousPush = UserConfigurationManager.AllowAnonymousPush,
                RepositoryPath = UserConfigurationManager.Repositories,
                AllowAnonymousRegistration = UserConfigurationManager.AllowAnonymousRegistration,
                AllowUserRepositoryCreation = UserConfigurationManager.AllowUserRepositoryCreation,
            });
        }

        [HttpPost]
        [AuthorizeRedirect(Roles = Definitions.Roles.Administrator)]
        public ActionResult Index(GlobalSettingsModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Directory.Exists(model.RepositoryPath))
                    {
                        System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(model.RepositoryPath);

                        UserConfigurationManager.AllowAnonymousPush = model.AllowAnonymousPush;
                        UserConfigurationManager.Repositories = model.RepositoryPath;
                        UserConfigurationManager.AllowAnonymousRegistration = model.AllowAnonymousRegistration;
                        UserConfigurationManager.AllowUserRepositoryCreation = model.AllowUserRepositoryCreation;
                        UserConfigurationManager.Save();

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
