using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;

using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Helpers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;

using Bonobo.Git.Server.Owin.Windows;
using System.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace Bonobo.Git.Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly BonoboGitServerContext _context;
        public IMembershipService MembershipService { get; set; }
        public IAuthenticationProvider AuthenticationProvider { get; set; }
        public IDatabaseResetManager ResetManager { get; set; }

        public HomeController(IMembershipService membershipService, IAuthenticationProvider authenticationProvider,
            IDatabaseResetManager resetManager, BonoboGitServerContext context)
        {
            MembershipService = membershipService;
            AuthenticationProvider = authenticationProvider;
            ResetManager = resetManager;
            _context = context;
        }

        [Authorize(Policy = "Web")]
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
            var cacheObj = Program.Cache[HttpUtility.UrlDecode(digest)];
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
                var user = _context.Users.FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                {
                    throw new UnauthorizedAccessException("Unknown user " + username);
                }
                return View(new ResetPasswordModel { Username = username, Digest = digest});
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
                var user = _context.Users.FirstOrDefault(x => x.Username.Equals(model.Username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                {
                    TempData["ResetSuccess"] = false;
                    Log.Warning("FAILURE");
                }
                else
                {
                    MembershipService.UpdateUser(user.Id, null, null, null, null, model.Password);
                    TempData["ResetSuccess"] = true;
                }
            }
            return View(model);
        }

        public ActionResult ForgotPassword()
        {
            return View(new ForgotPasswordModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = MembershipService.GetUserModel(model.Username);
                if (user == null)
                {
                    ModelState.AddModelError("", Resources.Home_ForgotPassword_UserNameFailure);
                    Log.Warning("FAILURE");
                }
                else
                {
                    string token = MembershipService.GenerateResetToken(user.Username);
                    Program.Cache.Add(token, model.Username, DateTimeOffset.Now.AddHours(1));

                    // Passing Requust.Url.Scheme to Url.Action forces it to generate a full URL
                    var resetUrl = Url.Action("ResetPassword", "Home", new {digest = HttpUtility.UrlEncode(Encoding.UTF8.GetBytes(token))},Request.Scheme);

                    TempData["SendSuccess"] = MembershipHelper.SendForgotPasswordEmail(user, resetUrl);
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

                HttpContext.ChallengeAsync(WindowsAuthenticationDefaults.AuthenticationType, authenticationProperties);
                return new EmptyResult();
            }

            return Redirect(returnUrl);
        }

        public ActionResult LogOn(string returnUrl)
        {
            return View(new LogOnModel { ReturnUrl = returnUrl });
        }

        public ActionResult LogOnWithResetOption(string returnUrl)
        {
            return View("LogOn", new LogOnModel { ReturnUrl = returnUrl, DatabaseResetCode = -1 });
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
                        AuthenticationProvider.SignIn(model.Username, Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl : Url.Action("Index", "Home"), model.RememberMe);
                        Log.Information("SUCCESS");
                        if (Url.IsLocalUrl(Request.GetEncodedUrl()) && model.DatabaseResetCode > 0 && model.Username == "admin" && ConfigurationManager.AppSettings["AllowDBReset"] == "true" )
                        {
                            ResetManager.DoReset(model.DatabaseResetCode);
                        }
                        return new EmptyResult();
                    case ValidationResult.NotAuthorized:
                        return new RedirectResult("~/Home/Unauthorized");
                    default:
                        ModelState.AddModelError("", Resources.Home_LogOn_UsernamePasswordIncorrect);
                        Log.Warning("FAILURE");
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

        public new ActionResult Unauthorized()
        {
            return View();
        }

        public ActionResult ChangeCulture(string lang, string returnUrl)
        {
            HttpContext.Session.SetString("Culture", JsonConvert.SerializeObject(new CultureInfo(lang)));
            return Redirect(returnUrl);
        }

        public ActionResult Diagnostics()
        {
            if (Url.IsLocalUrl(Request.GetEncodedUrl()))
            {
                var verifier = new DiagnosticReporter(HttpContext.RequestServices.GetService<IHostingEnvironment>());
                return Content(verifier.GetVerificationReport(HttpContext.RequestServices), "text/plain", Encoding.UTF8);
            }
            else
            {
                return Content("You can only run the diagnostics locally to the server");
            }
        }

    }
}
