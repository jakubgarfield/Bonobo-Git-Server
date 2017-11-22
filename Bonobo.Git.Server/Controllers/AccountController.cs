using System;
using System.Linq;
using System.Security.Claims;
using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Helpers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;

namespace Bonobo.Git.Server.Controllers
{
    public class AccountController : Controller
    {
        private readonly IOptions<AuthenticationSettings> _authSettings;

        public IMembershipService MembershipService { get; set; }
        public IRoleProvider RoleProvider { get; set; }
        public IAuthenticationProvider AuthenticationProvider { get; set; }

        public AccountController(
            IMembershipService membershipService,
            IRoleProvider roleProvider,
            IAuthenticationProvider authenticationProvider,
            IOptions<AuthenticationSettings> authSettings
            )
        {
            MembershipService = membershipService;
            RoleProvider = roleProvider;
            AuthenticationProvider = authenticationProvider;
            _authSettings = authSettings;
        }

        [WebAuthorize]
        public ActionResult Detail(Guid id)
        {
            UserModel user = MembershipService.GetUserModel(id);
            if (user != null)
            {
                var model = new UserDetailModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    Name = user.GivenName,
                    Surname = user.Surname,
                    Email = user.Email,
                    Roles = RoleProvider.GetRolesForUser(user.Id),
                    IsReadOnly = MembershipService.IsReadOnly()
                };
                return View(model);
            }
            return View();
        }

        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Delete(Guid id)
        {
            var user = MembershipService.GetUserModel(id);
            if (user != null)
            {
                return View(user);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Delete(UserDetailModel model)
        {
            if (model != null && model.Id != null)
            {
                if (model.Id != User.Id())
                {
                    var user = MembershipService.GetUserModel(model.Id);
                    MembershipService.DeleteUser(user.Id);
                    TempData["DeleteSuccess"] = true;
                }
                else
                {
                    TempData["DeleteSuccess"] = false;
                }
            }
            return RedirectToAction("Index");
        }

        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Index()
        {
            return View(GetDetailUsers());
        }

        [WebAuthorize]
        public ActionResult Edit(Guid id)
        {
            if (id != User.Id() && !User.IsInRole(Definitions.Roles.Administrator))
            {
                return RedirectToAction("Unauthorized", "Home");
            }

            if (MembershipService.IsReadOnly())
            {
                return RedirectToAction("Detail", "Account", new { id = id });
            }

            var user = MembershipService.GetUserModel(id);
            if (user != null)
            {
                var model = new UserEditModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    Name = user.GivenName,
                    Surname = user.Surname,
                    Email = user.Email,
                    Roles = RoleProvider.GetAllRoles(),
                    SelectedRoles = RoleProvider.GetRolesForUser(user.Id)
                };
                return View(model);
            }
            return View();
        }

        [HttpPost]
        [WebAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserEditModel model)
        {
            if (User.Id() != model.Id && !User.IsInRole(Definitions.Roles.Administrator))
            {
                return RedirectToAction("Unauthorized", "Home");
            }

            if (_authSettings.Value.DemoModeActive && User.IsInRole(Definitions.Roles.Administrator) && User.Id() == model.Id)
            {
                // Don't allow the admin user to be changed in demo mode
                return RedirectToAction("Unauthorized", "Home");
            }

            if (ModelState.IsValid)
            {
                bool valid = true;

                if (!User.IsInRole(Definitions.Roles.Administrator) && (model.OldPassword == null && model.NewPassword != null))
                {
                    ModelState.AddModelError("OldPassword", Resources.Account_Edit_OldPasswordEmpty);
                    valid = false;
                }

                if (model.OldPassword != null && MembershipService.ValidateUser(model.Username, model.OldPassword) != ValidationResult.Success)
                {
                    ModelState.AddModelError("OldPassword", Resources.Account_Edit_OldPasswordIncorrect);
                    valid = false;
                }

                if (User.IsInRole(Definitions.Roles.Administrator) && model.Id == User.Id() && !(model.PostedSelectedRoles != null && model.PostedSelectedRoles.Contains(Definitions.Roles.Administrator)))
                {
                    ModelState.AddModelError("Roles", Resources.Account_Edit_CannotRemoveYourselfFromAdminRole);
                    valid = false;
                }

                if (valid)
                {
                    MembershipService.UpdateUser(model.Id, model.Username, model.Name, model.Surname, model.Email, model.NewPassword);
                    RoleProvider.RemoveUserFromRoles(model.Id, RoleProvider.GetAllRoles());
                    if (model.PostedSelectedRoles != null)
                    {
                        RoleProvider.AddUserToRoles(model.Id, model.PostedSelectedRoles);
                    }
                    ViewBag.UpdateSuccess = true;
                }
            }

            model.Roles = RoleProvider.GetAllRoles();
            model.SelectedRoles = model.PostedSelectedRoles;

            return View(model);
        }

        public ActionResult CreateADUser([FromServices] ADHelper adHelper)
        {
            var efms = MembershipService as EFMembershipService;

            if ((!User.Identity.IsAuthenticated) || efms == null)
            {
                Log.Warning("CreateADUser: can't run IsAuth: {IsAuth}, MemServ {MemServ}",
                    User.Identity.IsAuthenticated,
                    MembershipService.GetType());
                return RedirectToAction("Unauthorized", "Home");
            }

            var credentials = User.Username();
            var adUser = adHelper.GetUserPrincipal(credentials);
            if (adUser != null)
            {
                var userId = adUser.Guid.GetValueOrDefault(Guid.NewGuid());
                if (MembershipService.CreateUser(credentials, Guid.NewGuid().ToString(), adUser.GivenName, adUser.Surname, adUser.EmailAddress, userId))
                {
                    // 2 because we just added the user and there is the default admin user.
                    if (_authSettings.Value.ImportWindowsAuthUsersAsAdmin || efms.UserCount() == 2)
                    {
                        Log.Information("Making AD user {User} into an admin", credentials);

                        var id = MembershipService.GetUserModel(credentials).Id;
                        RoleProvider.AddUserToRoles(id, new[] { Definitions.Roles.Administrator });

                        // Add the administrator role to the Identity/cookie
                        var Identity = (ClaimsIdentity)User.Identity;
                        Identity.AddClaim(new Claim(ClaimTypes.Role, Definitions.Roles.Administrator));
                        throw new NotImplementedException();
                        //var AuthenticationManager = HttpContext.GetOwinContext().Authentication;
                        //AuthenticationManager.AuthenticationResponseGrant = new AuthenticationResponseGrant(new ClaimsPrincipal(Identity), new AuthenticationProperties { IsPersistent = true });
                    }

                    return RedirectToAction("Index", "Repository");
                }
                else
                {
                    ModelState.AddModelError("Username", Resources.Account_Create_AccountAlreadyExists);
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("Unauthorized", "Home");
            }
        }

        public ActionResult Create()
        {
            if ((Request.HttpContext.User.Identity.IsAuthenticated && !User.IsInRole(Definitions.Roles.Administrator))
                || (!Request.HttpContext.User.Identity.IsAuthenticated && !UserConfiguration.Current.AllowAnonymousRegistration))
            {
                return RedirectToAction("Unauthorized", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserCreateModel model)
        {
            if ((Request.HttpContext.User.Identity.IsAuthenticated && !User.IsInRole(Definitions.Roles.Administrator))
                || (!Request.HttpContext.User.Identity.IsAuthenticated && !UserConfiguration.Current.AllowAnonymousRegistration))
            {
                return RedirectToAction("Unauthorized", "Home");
            }

            while (!String.IsNullOrEmpty(model.Username) && model.Username.Last() == ' ')
            {
                model.Username = model.Username.Substring(0, model.Username.Length - 1);
            }

            if (ModelState.IsValid)
            {
                if (MembershipService.CreateUser(model.Username, model.Password, model.Name, model.Surname, model.Email))
                {
                    if (User.IsInRole(Definitions.Roles.Administrator))
                    {
                        TempData["CreateSuccess"] = true;
                        TempData["NewUserId"] = MembershipService.GetUserModel(model.Username).Id;
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        AuthenticationProvider.SignIn(this.HttpContext, model.Username, Url.Action("Index", "Home"), false);
                        return new EmptyResult();
                    }
                }
                else
                {
                    ModelState.AddModelError("Username", Resources.Account_Create_AccountAlreadyExists);
                }
            }

            return View(model);
        }

        private UserDetailModelList GetDetailUsers()
        {
            var users = MembershipService.GetAllUsers();
            var model = new UserDetailModelList();
            model.IsReadOnly = MembershipService.IsReadOnly();
            foreach (var user in users)
            {
                model.Add(new UserDetailModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    Name = user.GivenName,
                    Surname = user.Surname,
                    Email = user.Email,
                    Roles = RoleProvider.GetRolesForUser(user.Id),
                    IsReadOnly = model.IsReadOnly
                });
            }
            return model;
        }

    }
}
