using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Bonobo.Git.Server.Security;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.App_GlobalResources;
using System.Globalization;

namespace Bonobo.Git.Server.Controllers
{
    public class HomeController : Controller
    {
        [Dependency]
        public IMembershipService MembershipService { get; set; }

        [Dependency]
        public IFormsAuthenticationService FormsAuthenticationService { get; set; }

        [FormsAuthorize]
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Repository");
        }

        [FormsAuthorize]
        public ActionResult About()
        {
            return View();
        }

        public ActionResult PageNotFound()
        {
            return View();
        }

        public ActionResult ServerError()
        {
            return View();
        }

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult LogOn(string returnUrl)
        {
            return View(new LogOnModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ValidateUser(model.Username, model.Password))
                {
                    FormsAuthenticationService.SignIn(model.Username, model.RememberMe);
                    if (Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", Resources.Home_LogOn_UsernamePasswordIncorrect);
                }
            }

            return View(model);
        }

        [FormsAuthorizeAttribute]
        public ActionResult LogOff()
        {
            FormsAuthenticationService.SignOut();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Unauthorized()
        {
            return View();
        }

        public ActionResult ChangeCulture(string lang, string returnUrl)
        {
            Session["Culture"] = new CultureInfo(lang);
            return Redirect(returnUrl);
        }
    }
}
