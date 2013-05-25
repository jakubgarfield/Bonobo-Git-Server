using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Bonobo.Git.Server.App_GlobalResources;

namespace Bonobo.Git.Server.Models
{
    public class TeamModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Members { get; set; }
        public string[] Repositories { get; set; }
    }

    public class TeamDetailModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(40, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [Display(ResourceType = typeof(Resources), Name = "Team_Detail_Name")]
        public string Name { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Team_Detail_Description")]
        public string Description { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Team_Detail_Members")]
        public string[] Members { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Team_Detail_Repositories")]
        public string[] Repositories { get; set; }
    }
}