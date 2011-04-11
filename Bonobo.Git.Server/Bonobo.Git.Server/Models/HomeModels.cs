using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Bonobo.Git.Server.App_GlobalResources;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Models
{
    public class LogOnModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [Display(ResourceType = typeof(Resources), Name = "Home_LogOn_Username")]
        public string Username { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resources), Name = "Home_LogOn_Password")]
        public string Password { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Home_LogOn_RememberMe")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [Display(ResourceType = typeof(Resources), Name = "Home_Register_Username")]
        public string Username { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [Display(ResourceType = typeof(Resources), Name = "Home_Register_Name")]
        public string Name { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [Display(ResourceType = typeof(Resources), Name = "Home_Register_Surname")]
        public string Surname { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [Email(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Email")]
        [DataType(DataType.EmailAddress)]
        [Display(ResourceType = typeof(Resources), Name = "Home_Register_Email")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resources), Name = "Home_Register_Password")]
        public string Password { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [Compare("Password", ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Compare")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resources), Name = "Home_Register_ConfirmPassword")]
        public string ConfirmPassword { get; set; }
    }
}