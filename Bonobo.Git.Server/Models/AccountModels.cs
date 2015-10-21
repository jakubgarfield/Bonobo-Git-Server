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
using Bonobo.Git.Server.Data;

namespace Bonobo.Git.Server.Models
{
    public class RoleModel : INameProperty
    {
        public string Name { get; set; }
        public string[] Members { get; set; }
    }

    public class UserModel : INameProperty
    {
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }

        public string DisplayName
        {
            get
            {
                return String.Format("{0} {1}", GivenName, Surname);
            }
        }
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

        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Compare")]
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
        public bool IsReadOnly { get; set; }
    }

    public class UserDetailModelList : List<UserDetailModel>
    {
        public bool IsReadOnly { get; set; }
    }

    public class UserCreateModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [Display(ResourceType = typeof(Resources), Name = "Account_Create_Username")]
        public string Username { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [Display(ResourceType = typeof(Resources), Name = "Account_Create_Name")]
        public string Name { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [Display(ResourceType = typeof(Resources), Name = "Account_Create_Surname")]
        public string Surname { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [Email(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Email")]
        [DataType(DataType.EmailAddress)]
        [Display(ResourceType = typeof(Resources), Name = "Account_Create_Email")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resources), Name = "Account_Create_Password")]
        public string Password { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Compare")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resources), Name = "Account_Create_ConfirmPassword")]
        public string ConfirmPassword { get; set; }
    }
}