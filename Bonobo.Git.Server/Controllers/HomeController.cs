using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Helpers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Practices.Unity;
using Bonobo.Git.Server.Owin.Windows;

namespace Bonobo.Git.Server.Controllers
{
    public class HomeController : Controller
    {
        [Dependency]
        public IMembershipService MembershipService { get; set; }

        [Dependency]
        public IAuthenticationProvider AuthenticationProvider { get; set; }

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

        private string CheckForPasswordResetUsername(string digest)
        {
            var cacheObj = MvcApplication.Cache[HttpUtility.UrlDecode(digest)];
            if (cacheObj == null)
            {
                return null;
            }
            return cacheObj.ToString();
        }

        public ActionResult ResetPassword(string digest)
        {
            string username = CheckForPasswordResetUsername(digest);
            if (username != null )
            {
                using (var db = new BonoboGitServerContext())
                {
                    var user = db.Users.FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                    if (user == null)
                    {
                        throw new UnauthorizedAccessException("Unknown user " + username);
                    }
                    return View(new ResetPasswordModel { Username = username, Digest = digest});
                }
            }
            else
            {
                ModelState.AddModelError("", "Password reset link was not valid");
                return RedirectToAction("Index", "Home");    
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var cachedUsername = CheckForPasswordResetUsername(model.Digest);
                if (cachedUsername == null || cachedUsername != model.Username)
                {
                    throw new UnauthorizedAccessException("Invalid password reset form");
                }
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
        [ValidateAntiForgeryToken]
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

        [AllowAnonymous]
        [HttpGet]
        public ActionResult WindowsLogin(string returnUrl)
        {
            if (String.IsNullOrEmpty(User.Identity.Name))
            {
                AuthenticationProperties authenticationProperties = new AuthenticationProperties()
                {
                    RedirectUri = returnUrl
                };

                Request.GetOwinContext().Authentication.Challenge(authenticationProperties, WindowsAuthenticationDefaults.AuthenticationType);
                return new EmptyResult();
            }

            return Redirect(returnUrl);
        }

        public ActionResult LogOn(string returnUrl)
        {
            return View(new LogOnModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOn(LogOnModel model)
        {
            if (ModelState.IsValid)
            {
                ValidationResult result = MembershipService.ValidateUser(model.Username, model.Password);
                switch (result)
                {
                    case ValidationResult.Success:
                        AuthenticationProvider.SignIn(model.Username, Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl : Url.Action("Index", "Home"));
                        Response.AppendToLog("SUCCESS");
                        return new EmptyResult();
                    case ValidationResult.NotAuthorized:
                        return new RedirectResult("~/Home/Unauthorized");
                    default:
                        ModelState.AddModelError("", Resources.Home_LogOn_UsernamePasswordIncorrect);
                        Response.AppendToLog("FAILURE");
                        break;
                }                
            }

            return View(model);
        }

        public ActionResult LogOff()
        {
            AuthenticationProvider.SignOut();
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
