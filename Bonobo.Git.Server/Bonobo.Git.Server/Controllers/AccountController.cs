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

namespace Bonobo.Git.Server.Controllers
{
    public class AccountController : Controller
    {
        [Dependency]
        public IMembershipService MembershipService { get; set; }

        [Dependency]
        public IFormsAuthenticationService FormsAuthenticationService { get; set; }

        public ActionResult Detail(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
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

        [AuthorizeRedirect(Roles = Definitions.Roles.Administrator)]
        public ActionResult Delete(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                if (id != User.Identity.Name)
                {
                    MembershipService.DeleteUser(id);
                    ViewBag.DeleteSuccess = true;
                }
                else
                {
                    ViewBag.DeleteSuccess = false;
                }
            }
            return View("Index", GetDetailUsers());
        }

        [AuthorizeRedirect(Roles = Definitions.Roles.Administrator)]
        public ActionResult Index()
        {
            return View( GetDetailUsers());
        }

        public ActionResult Edit(string id)
        {
            if (User.Identity.Name != id && !User.IsInRole(Definitions.Roles.Administrator))
            {
                return new RedirectResult("Unauthorized");
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
        public ActionResult Edit(UserEditModel model)
        {
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

                if (User.IsInRole(Definitions.Roles.Administrator) && model.Username == User.Identity.Name && !(model.Roles != null && model.Roles.Contains(Definitions.Roles.Administrator)))
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
