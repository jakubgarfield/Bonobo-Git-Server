using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Data;

namespace Bonobo.Git.Server.Models
{
    public class RoleModel : INameProperty
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid[] Members { get; set; }
        public string DisplayName
        {
            get
            {
                return Name;
            }
        }
    }

    public class UserModel : INameProperty
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }

        public string DisplayName
        {
            get
            {
                var compositeName = String.Format("{0} {1}", GivenName, Surname).Trim();
                if (String.IsNullOrEmpty(compositeName))
                {
                    // Return the username if we don't have a GivenName or Surname
                    return Username;
                }
                else
                {
                    return compositeName;
                }
            }
        }

        string INameProperty.Name
        {
            get { return Username; }
        }

        /// <summary>
        /// This is the name we'd sort users by
        /// </summary>
        public string SortName
        {
            get
            {
                var compositeName = Surname + GivenName;
                if (String.IsNullOrEmpty(compositeName))
                {
                    return Username;
                }
                return compositeName;
            }
        }
    }

    public class UserEditModel
    {
        public Guid Id { get; set; }

        [Remote("UniqueNameUser", "Validation", AdditionalFields="Id", ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Duplicate_Name")]
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [Display(ResourceType = typeof(Resources), Name = "Account_Edit_Username")]
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

        public string[] SelectedRoles { get; set; }
        public string[] PostedSelectedRoles { get; set; }
    }

    public class UserDetailModel
    {
        public Guid Id { get; set; }

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
        [Remote("UniqueNameUser", "Validation", AdditionalFields="Id", ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Duplicate_Name")]
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