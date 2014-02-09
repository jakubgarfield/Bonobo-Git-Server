using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Bonobo.Git.Server.App_GlobalResources;
using LibGit2Sharp;
using System.Text;

namespace Bonobo.Git.Server.Models
{
    public class RepositoryModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool AnonymousAccess { get; set; }
        public string[] Users { get; set; }
        public string[] Administrators { get; set; }
        public string[] Teams { get; set; }
    }

    public class RepositoryDetailModel
    {
        [FileName(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_FileName")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [Display(ResourceType = typeof(Resources), Name = "Repository_Detail_Name")]
        public string Name { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Detail_Description")]
        public string Description { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Detail_Users")]
        public string[] Users { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Detail_Teams")]
        public string[] Teams { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
        [Display(ResourceType = typeof(Resources), Name = "Repository_Detail_Administrators")]
        public string[] Administrators { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Detail_IsCurrentUserAdmin")]
        public bool IsCurrentUserAdministrator { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Detail_Anonymous")]
        public bool AllowAnonymous { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Detail_Status")]
        public RepositoryDetailStatus Status { get; set; }

    }

    public enum RepositoryDetailStatus
    {
        Unknown = 0,
        Valid,
        Missing
    }

    public class RepositoryTreeDetailModel
    {
        [Display(ResourceType = typeof(Resources), Name = "Repository_Tree_Name")]
        public string Name { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Tree_CommitMessage")]
        public string CommitMessage { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Tree_CommitDate")]
        public DateTime? CommitDate { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Tree_Author")]
        public string Author { get; set; }

        public bool IsTree { get; set; }
        public string TreeName { get; set; }
        public bool IsImage { get; set; }
        public bool IsText { get; set; }
        public string Path { get; set; }
        public byte[] Data { get; set; }
        public string Text { get; set; }
        public string TextBrush { get; set; }
        public Encoding Encoding { get; set; }
    }

    public class RepositoryTreeModel
    {
        public string Name { get; set; }
        public IEnumerable<RepositoryTreeDetailModel> Files { get; set; }
    }

    public class RepositoryCommitsModel
    {
        public string Name { get; set; }
        public IEnumerable<RepositoryCommitModel> Commits { get; set; }
    }

    public class RepositoryCommitChangeModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ChangeKind Status { get; set; }        
    }

    public class RepositoryCommitModel
    {
        public string Name { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_ID")]
        public string ID { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_TreeID")]
        public string TreeID { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_Parents")]
        public string[] Parents { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_Author")]
        public string Author { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_AuthorEmail")]
        public string AuthorEmail { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_Date")]
        public DateTime Date { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_Message")]
        public string Message { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_Changes")]
        public IEnumerable<RepositoryCommitChangeModel> Changes { get; set; }

    }
}