﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Data;

namespace Bonobo.Git.Server.Models
{
    using IdName = Tuple<int, string, string>;
    using System.Web.Mvc;

    public class TeamModel : INameProperty 
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public UserModel[] Members { get; set; }
        public string DisplayName
        {
            get
            {
                return Name;
            }
        }
    }

    public class TeamEditModel
    {
        public Guid Id { get; set; }

        [Remote("UniqueNameTeam", "Validation", AdditionalFields="Id", ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Duplicate_Name")]
        [AllowHtml]
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(40, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [Display(ResourceType = typeof(Resources), Name = "Team_Detail_Name")]
        public string Name { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resources), Name = "Team_Detail_Description")]
        public string Description { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Team_Detail_Members")]
        public UserModel[] AllUsers { get; set; }

        public UserModel[] SelectedUsers { get; set; }

        public Guid[] PostedSelectedUsers { get; set; }
    }


    public class TeamDetailModel
    {
        public Guid Id { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Team_Detail_Name")]
        public string Name { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Team_Detail_Description")]
        public string Description { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Team_Detail_Members")]
        public UserModel[] Members { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Team_Detail_Repositories")]
        public RepositoryModel[] Repositories { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public class TeamDetailModelList : List<TeamDetailModel>
    {
        public bool IsReadOnly { get; set; }
    }
}