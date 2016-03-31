using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Extensions;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;

using Bonobo.Git.Server.Helpers;
using System.DirectoryServices.AccountManagement;

using Microsoft.Practices.Unity;

namespace Bonobo.Git.Server.Controllers
{
    public class AccountController : Controller
    {
        [Dependency]
        public IMembershipService MembershipService { get; set; }

        [Dependency]
        public IRoleProvider RoleProvider { get; set; }

        [Dependency]
        public IAuthenticationProvider AuthenticationProvider { get; set; }

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

        public ActionResult CreateADUser()
        {
            if ((!Request.IsAuthenticated) || !(MembershipService is EFMembershipService))
            {
                return RedirectToAction("Unauthorized", "Home");
            }

            var credentials = User.Username();
            var dc = new PrincipalContext(ContextType.Domain, credentials.GetDomain());
            var adUser = UserPrincipal.FindByIdentity(dc, credentials);
            if (adUser != null)
            {
                //TODO Is this legit? Could an AD user ever not have a Guid
                var userId = adUser.Guid.GetValueOrDefault(Guid.NewGuid());
                if (MembershipService.CreateUser(credentials, Guid.NewGuid().ToString(), adUser.GivenName, adUser.Surname, adUser.EmailAddress, userId))
                {
                    if (MembershipService is EFMembershipService)
                    {
                        var efms = MembershipService as EFMembershipService;
                        // 2 because we just added the user and there is the default admin user.
                        if ((AuthenticationSettings.ImportWindowsAuthUsersAsAdmin || efms.UserCount() == 2))
                        {
                            var id = MembershipService.GetUserModel(credentials).Id;
                            RoleProvider.AddUserToRoles(id, new string[] {Definitions.Roles.Administrator});
                        }
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
            if ((Request.IsAuthenticated && !User.IsInRole(Definitions.Roles.Administrator)) || (!Request.IsAuthenticated && !UserConfiguration.Current.AllowAnonymousRegistration))
            {
                return RedirectToAction("Unauthorized", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserCreateModel model)
        {
            if ((Request.IsAuthenticated && !User.IsInRole(Definitions.Roles.Administrator)) || (!Request.IsAuthenticated && !UserConfiguration.Current.AllowAnonymousRegistration))
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
                        AuthenticationProvider.SignIn(model.Username, Url.Action("Index", "Home"), false);
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
