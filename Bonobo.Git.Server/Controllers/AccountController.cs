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
        public ActionResult Detail(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                UserModel user = MembershipService.GetUser(id);
                if (user != null)
                {
                    var model = new UserDetailModel
                    {
                        Username = user.Name,
                        Name = user.GivenName,
                        Surname = user.Surname,
                        Email = user.Email,
                        Roles = RoleProvider.GetRolesForUser(user.Name),
                        IsReadOnly = MembershipService.IsReadOnly()
                    };
                    return View(model);
                }
            }
            return View();
        }

        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Delete(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                var user = MembershipService.GetUser(id);
                if (user != null)
                {
                    var model = new UserDetailModel
                    {
                        Username = user.Name,
                        Name = user.GivenName,
                        Surname = user.Surname,
                        Email = user.Email,
                        Roles = RoleProvider.GetRolesForUser(user.Name),
                        IsReadOnly = MembershipService.IsReadOnly()
                    };
                    return View(model);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Delete(UserDetailModel model)
        {
            if (model != null && !String.IsNullOrEmpty(model.Username))
            {
                if (!model.Username.Equals(User.Id(), StringComparison.OrdinalIgnoreCase))
                {
                    MembershipService.DeleteUser(model.Username);
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
        public ActionResult Edit(string id)
        {
            if (!id.Equals(User.Id(), StringComparison.OrdinalIgnoreCase) && !User.IsInRole(Definitions.Roles.Administrator))
            {
                return RedirectToAction("Unauthorized", "Home");
            }

            if (MembershipService.IsReadOnly())
            {
                return RedirectToAction("Detail", "Account", new { id = id });
            }

            if (!String.IsNullOrEmpty(id))
            {
                var user = MembershipService.GetUser(id);
                if (user != null)
                {
                    var model = new UserEditModel
                    {
                        Username = user.Name,
                        Name = user.GivenName,
                        Surname = user.Surname,
                        Email = user.Email,
                        Roles = RoleProvider.GetRolesForUser(user.Name),
                    };
                    PopulateRoles();
                    return View(model);
                }
            }
            return View();
        }

        [HttpPost]
        [WebAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserEditModel model)
        {
            if (!User.Id().Equals(model.Username, StringComparison.OrdinalIgnoreCase) && !User.IsInRole(Definitions.Roles.Administrator))
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

                if (User.IsInRole(Definitions.Roles.Administrator) && model.Username.Equals(User.Id(), StringComparison.OrdinalIgnoreCase) && !(model.Roles != null && model.Roles.Contains(Definitions.Roles.Administrator)))
                {
                    ModelState.AddModelError("Roles", Resources.Account_Edit_CannotRemoveYourselfFromAdminRole);
                    valid = false;
                }

                if (valid)
                {
                    MembershipService.UpdateUser(model.Username, model.Name, model.Surname, model.Email, model.NewPassword);
                    RoleProvider.RemoveUserFromRoles(model.Username, RoleProvider.GetAllRoles());
                    if (model.Roles != null)
                    {
                        RoleProvider.AddUserToRoles(model.Username, model.Roles);
                    }
                    ViewBag.UpdateSuccess = true;
                }
            }

            PopulateRoles();
            return View(model);
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
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        AuthenticationProvider.SignIn(model.Username, Url.Action("Index", "Home"));
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
                    Username = user.Name,
                    Name = user.GivenName,
                    Surname = user.Surname,
                    Email = user.Email,
                    Roles = RoleProvider.GetRolesForUser(user.Name),
                    IsReadOnly = model.IsReadOnly
                });
            }
            return model;
        }

        private void PopulateRoles()
        {
            ViewData["AvailableRoles"] = RoleProvider.GetAllRoles();
        }
    }
}
