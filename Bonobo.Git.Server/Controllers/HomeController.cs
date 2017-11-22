using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Extensions;
using Bonobo.Git.Server.Helpers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bonobo.Git.Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOptions<AppSettings> _appSettings;

        public HomeController(
            IMembershipService membershipService,
            IAuthenticationProvider authenticationProvider,
            IDatabaseResetManager resetManager,
            IOptions<AppSettings> appSettings)
        {
            MembershipService = membershipService;
            AuthenticationProvider = authenticationProvider;
            ResetManager = resetManager;
            _appSettings = appSettings;
        }

        public IMembershipService MembershipService { get; set; }
        public IAuthenticationProvider AuthenticationProvider { get; set; }
        public IDatabaseResetManager ResetManager { get; set; }

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Repository");
        }

        public IActionResult PageNotFound()
        {
            return View();
        }

        public IActionResult ServerError()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        private string CheckForPasswordResetUsername(string digest)
        {
            object cacheObj = null;// MvcApplication.Cache[HttpUtility.UrlDecode(digest)];
            if (cacheObj == null)
            {
                return null;
            }
            return cacheObj.ToString();
        }

        public ActionResult ResetPassword([FromServices]BonoboGitServerContext db, string digest)
        {
            string username = CheckForPasswordResetUsername(digest);
            if (username != null)
            {
                //using (var db = new BonoboGitServerContext())
                {
                    var user = db.Users.FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                    if (user == null)
                    {
                        throw new UnauthorizedAccessException("Unknown user " + username);
                    }
                    return View(new ResetPasswordModel { Username = username, Digest = digest });
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
        public ActionResult ResetPassword([FromServices]BonoboGitServerContext db, ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var cachedUsername = CheckForPasswordResetUsername(model.Digest);
                if (cachedUsername == null || cachedUsername != model.Username)
                {
                    throw new UnauthorizedAccessException("Invalid password reset form");
                }
                //using (var db = new BonoboGitServerContext())
                {
                    var user = db.Users.FirstOrDefault(x => x.Username.Equals(model.Username, StringComparison.OrdinalIgnoreCase));
                    if (user == null)
                    {
                        TempData["ResetSuccess"] = false;
                        //Response.AppendToLog("FAILURE");
                    }
                    else
                    {
                        MembershipService.UpdateUser(user.Id, null, null, null, null, model.Password);
                        TempData["ResetSuccess"] = true;
                    }
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
                    //Response.AppendToLog("FAILURE");
                }
                else
                {
                    string token = MembershipService.GenerateResetToken(user.Username);
                    //MvcApplication.Cache.Add(token, model.Username, DateTimeOffset.Now.AddHours(1));

                    // Passing Requust.Url.Scheme to Url.Action forces it to generate a full URL
                    var resetUrl = Url.Action("ResetPassword", "Home", new { digest = HttpUtility.UrlEncode(Encoding.UTF8.GetBytes(token)) }, Request.Scheme);

                    TempData["SendSuccess"] = MembershipHelper.SendForgotPasswordEmail(user, resetUrl);
                }
            }
            return View(model);
        }

        //[AllowAnonymous]
        //[HttpGet]
        //public ActionResult WindowsLogin(string returnUrl)
        //{
        //    if (String.IsNullOrEmpty(User.Identity.Name))
        //    {
        //        AuthenticationProperties authenticationProperties = new AuthenticationProperties()
        //        {
        //            RedirectUri = returnUrl
        //        };

        //        Request.GetOwinContext().Authentication.Challenge(authenticationProperties, WindowsAuthenticationDefaults.AuthenticationType);
        //        return new EmptyResult();
        //    }

        //    return Redirect(returnUrl);
        //}

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
        public async Task<ActionResult> LogOn(LogOnModel model)
        {
            if (ModelState.IsValid)
            {
                ValidationResult result = MembershipService.ValidateUser(model.Username, model.Password);
                switch (result)
                {
                    case ValidationResult.Success:
                        await AuthenticationProvider.SignIn(HttpContext, model.Username, Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl : Url.Action("Index", "Home"), model.RememberMe);
                        //Response.AppendToLog("SUCCESS");
                        if (HttpContext.IsLocal() && model.DatabaseResetCode > 0 && model.Username == "admin" && _appSettings.Value.AllowDBReset)
                        {
                            ResetManager.DoReset(model.DatabaseResetCode);
                        }
                        return new EmptyResult();
                    case ValidationResult.NotAuthorized:
                        return new RedirectResult("~/Home/Unauthorized");
                    default:
                        ModelState.AddModelError("", Resources.Home_LogOn_UsernamePasswordIncorrect);
                        //Response.AppendToLog("FAILURE");
                        break;
                }
            }

            return View(model);
        }

        public ActionResult LogOff()
        {
            AuthenticationProvider.SignOut(HttpContext);
            return RedirectToAction("Index", "Home");
        }

        public new ActionResult Unauthorized()
        {
            return View();
        }

        public ActionResult ChangeCulture(string lang, string returnUrl)
        {
            //Session["Culture"] = new CultureInfo(lang);
            return Redirect(returnUrl);
        }

        public ActionResult Diagnostics([FromServices]DiagnosticReporter verifier)
        {
            if (HttpContext.IsLocal())
            {
                return Content(verifier.GetVerificationReport(), "text/plain", Encoding.UTF8);
            }
            else
            {
                return Content("You can only run the diagnostics locally to the server");
            }
        }

    }
}
