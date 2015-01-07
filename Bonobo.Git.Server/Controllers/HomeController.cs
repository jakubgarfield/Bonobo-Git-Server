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
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Helpers;
using System.Text;
using System.Web.Caching;

namespace Bonobo.Git.Server.Controllers
{
    public class HomeController : Controller
    {
        [Dependency]
        public IMembershipService MembershipService { get; set; }

        [Dependency]
        public IFormsAuthenticationService FormsAuthenticationService { get; set; }


        [WebAuthorize]
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Repository");
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

        public ActionResult ResetPassword(string digest)
        {
            string username;
            digest = HttpUtility.UrlDecode(digest);
            var cacheObj = MvcApplication.Cache[digest];
            if ( cacheObj != null )
            {
                using (var db = new BonoboGitServerContext())
                {
                    username = cacheObj.ToString();
                    var user = db.Users.FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                    return View(new ResetPasswordModel { Username = username });
                }
            }
            else
            {
                ModelState.AddModelError("", "Password reset link was not valid");
                return RedirectToAction("Index", "Home");    
            }
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new BonoboGitServerContext())
                {
                    var user = db.Users.FirstOrDefault(x => x.Username.Equals(model.Username, StringComparison.OrdinalIgnoreCase));
                    if (user == null)
                    {
                        TempData["ResetSuccess"] = false;
                        Response.AppendToLog("FAILURE");
                    }
                    else
                    {
                        MembershipService.UpdateUser(model.Username, user.Name, user.Surname, user.Email, model.Password);
                        TempData["ResetSuccess"] = true;
                    }
                }
            }
            return View(model);
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new BonoboGitServerContext())
                {
                    var user = db.Users.FirstOrDefault(x => x.Username.Equals(model.Username, StringComparison.OrdinalIgnoreCase));
                    if (user == null)
                    {
                        
                        ModelState.AddModelError("", Resources.Home_ForgotPassword_UserNameFailure);
                        Response.AppendToLog("FAILURE");
                    }
                    else
                    {
                        string token = MembershipService.GenerateResetToken(model.Username);
                        MvcApplication.Cache.Add(token, model.Username, DateTimeOffset.Now.AddHours(1));
                        TempData["SendSuccess"] = MembershipHelper.SendForgotPasswordEmail(user, token);
                    }
                }
            }
            return View(model);
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
                    Response.AppendToLog("SUCCESS");
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
                    Response.AppendToLog("FAILURE");
                }
            }

            return View(model);
        }

        [WebAuthorizeAttribute]
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
