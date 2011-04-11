using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Bonobo.Git.Server.App_GlobalResources;

namespace Bonobo.Git.Server.Models
{
    public class UserModel
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; }
    }

    public class UserEditModel
    {
        public string Username { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [Display(ResourceType = typeof(Resources), Name = "Account_Edit_Name")]
        public string Name { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [Display(ResourceType = typeof(Resources), Name = "Account_Edit_Surname")]
        public string Surname { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [Email(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Email")]
        [DataType(DataType.EmailAddress)]
        [Display(ResourceType = typeof(Resources), Name = "Account_Edit_Email")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resources), Name = "Account_Edit_CurrentPassword")]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resources), Name = "Account_Edit_NewPassword")]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Compare")]
        [Display(ResourceType = typeof(Resources), Name = "Account_Edit_ConfirmPassword")]
        public string ConfirmPassword { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Account_Edit_Roles")]
        public string[] Roles { get; set; }
    }    

    public class UserDetailModel
    {
        [Display(ResourceType = typeof(Resources), Name = "Account_Detail_Username")]
        public string Username { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Account_Detail_Name")]
        public string Name { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Account_Detail_Surname")]
        public string Surname { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Account_Detail_Email")]
        public string Email { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Account_Detail_Roles")]
        public string[] Roles { get; set; }
    }
}
