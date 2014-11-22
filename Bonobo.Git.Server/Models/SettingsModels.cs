using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Bonobo.Git.Server.App_GlobalResources;

namespace Bonobo.Git.Server.Models
{
    public class GlobalSettingsModel
    {
        [Display(ResourceType = typeof(Resources), Name = "Settings_Global_AllowAnonymousPush")]
        public bool AllowAnonymousPush { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Settings_Global_AllowAnonymousRegistration")]
        public bool AllowAnonymousRegistration { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Settings_Global_AllowUserRepositoryCreation")]
        public bool AllowUserRepositoryCreation { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [Display(ResourceType = typeof(Resources), Name = "Settings_Global_RepositoryPath")]
        public string RepositoryPath { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Settings_Global_DefaultLanguage")]
        public string DefaultLanguage { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Settings_Global_SiteTitle")]
        public string SiteTitle { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Settings_Global_SiteLogoUrl")]
        public string SiteLogoUrl { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Settings_Global_SiteFooterMessage")]
        public string SiteFooterMessage { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Settings_Global_IsCommitAuthorAvatarVisible")]
        public bool IsCommitAuthorAvatarVisible { get; set; }
    }   
}