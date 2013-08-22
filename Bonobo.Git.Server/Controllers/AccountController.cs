using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bonobo.Git.Server.Models;
using System.Web.Security;
using Bonobo.Git.Server.Security;
using Microsoft.Practices.Unity;
using System.Globalization;
using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Extensions;

namespace Bonobo.Git.Server.Controllers
{
    public class AccountController : Controller
    {
        [Dependency]
        public IMembershipService MembershipService { get; set; }

        [Dependency]
        public IFormsAuthenticationService FormsAuthenticationService { get; set; }


        [WebAuthorizeAttribute]
        public ActionResult Detail(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                id = UsernameUrl.Decode(id);
                var user = MembershipService.GetUser(id);
                if (user != null)
                {
                    var model = new UserDetailModel
                    {
                        Username = user.Username,
                        Name = user.Name,
                        Surname = user.Surname,
                        Email = user.Email,
                        Roles = user.Roles,                        
                    };
                    return View(model);
                }
            }
            return View();
        }

        [WebAuthorizeAttribute(Roles = Definitions.Roles.Administrator)]
        public ActionResult Delete(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                id = UsernameUrl.Decode(id).ToLowerInvariant();
                return View(new UserDetailModel { Username = id });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [WebAuthorizeAttribute(Roles = Definitions.Roles.Administrator)]
        public ActionResult Delete(UserDetailModel model)
        {
            if (model != null && !String.IsNullOrEmpty(model.Username))
            {
                if (model.Username != User.Identity.Name.ToLowerInvariant())
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

        [WebAuthorizeAttribute(Roles = Definitions.Roles.Administrator)]
        public ActionResult Index()
        {
            return View(GetDetailUsers());
        }

        [WebAuthorizeAttribute]
        public ActionResult Edit(string id)
        {
            id = UsernameUrl.Decode(id).ToLowerInvariant();

            if (User.Identity.Name.ToLowerInvariant() != id && !User.IsInRole(Definitions.Roles.Administrator))
            {
                return RedirectToAction("Unauthorized", "Home");
            }

            if (!String.IsNullOrEmpty(id))
            {
                var user = MembershipService.GetUser(id);
                if (user != null)
                {
                    var roles = Roles.GetRolesForUser(id);

                    var model = new UserEditModel
                    {
                        Username = id,
                        Name = user.Name,
                        Surname = user.Surname,
                        Email = user.Email,
                        Roles = roles,
                    };
                    PopulateRoles();
                    return View(model);
                }
            }
            return View();
        }

        [HttpPost]
        [WebAuthorizeAttribute]
        public ActionResult Edit(UserEditModel model)
        {
            if (User.Identity.Name.ToLowerInvariant() != model.Username && !User.IsInRole(Definitions.Roles.Administrator))
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

                if (model.OldPassword != null && !MembershipService.ValidateUser(model.Username, model.OldPassword))
                {
                    ModelState.AddModelError("OldPassword", Resources.Account_Edit_OldPasswordIncorrect);
                    valid = false;
                }

                if (User.IsInRole(Definitions.Roles.Administrator) && model.Username == User.Identity.Name.ToLowerInvariant() && !(model.Roles != null && model.Roles.Contains(Definitions.Roles.Administrator)))
                {
                    ModelState.AddModelError("Roles", Resources.Account_Edit_CannotRemoveYourselfFromAdminRole);
                    valid = false;
                }

                if (valid)
                {
                    MembershipService.UpdateUser(model.Username, model.Name, model.Surname, model.Email, model.NewPassword);
                    Roles.RemoveUserFromRoles(model.Username, Roles.GetAllRoles());
                    if (model.Roles != null)
                    {
                        Roles.AddUserToRoles(model.Username, model.Roles);
                    }
                    ViewBag.UpdateSuccess = true;
                }
            }

            PopulateRoles();
            return View(model);
        }

        [WindowsActionFilter]
        public ActionResult Create()
        {
            if ((Request.IsAuthenticated && !User.IsInRole(Definitions.Roles.Administrator)) || (!Request.IsAuthenticated && !UserConfiguration.Current.AllowAnonymousRegistration))
            {
                return RedirectToAction("Unauthorized", "Home");
            }

            return View();
        }

        [HttpPost, WindowsActionFilter]
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
                        FormsAuthenticationService.SignIn(model.Username, false);
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("Username", Resources.Account_Create_AccountAlreadyExists);
                }
            }

            return View(model);
        }

        private List<UserDetailModel> GetDetailUsers()
        {
            var users = MembershipService.GetAllUsers();
            var model = new List<UserDetailModel>();
            foreach (var item in users)
            {
                model.Add(new UserDetailModel
                {
                    Username = item.Username,
                    Name = item.Name,
                    Surname = item.Surname,
                    Email = item.Email,
                    Roles = item.Roles,
                });
            }
            return model;
        }

        private void PopulateRoles()
        {
            ViewData["AvailableRoles"] = Roles.GetAllRoles();
        }
    }
}
