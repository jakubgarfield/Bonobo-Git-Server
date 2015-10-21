using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;

using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Attributes;
using Bonobo.Git.Server.Data;

using LibGit2Sharp;

namespace Bonobo.Git.Server.Models
{
    public class RepositoryModel : INameProperty
    {
        public string Name { get; set; }
        public string Group { get; set; }
        public string Description { get; set; }
        public bool AnonymousAccess { get; set; }
        public string[] Users { get; set; }
        public string[] Administrators { get; set; }
        public string[] Teams { get; set; }
        public bool AuditPushUser { get; set; }
        public byte[] Logo { get; set; }
        public bool RemoveLogo { get; set; }
    }

    public class RepositoryDetailModel
    {
        [RegularExpression(@"([\w\.-])*([\w])$", ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_FileName_Regex")]
        [FileName(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_FileName")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        [Display(ResourceType = typeof(Resources), Name = "Repository_Detail_Name")]
        public string Name { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Detail_Group")]
        [StringLength(255, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
        public string Group { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Detail_Description")]
        [StringLength(255, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
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

        [Display(ResourceType = typeof(Resources), Name = "Repository_Detail_AuditPushUser")]
        public bool AuditPushUser { get; set; }

        public RepositoryLogoDetailModel Logo { get; set; }
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
        public string CommitDateString { get { return CommitDate.HasValue ? CommitDate.Value.ToString(CultureInfo.InstalledUICulture) : CommitDate.ToString(); } }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Tree_Author")]
        public string Author { get; set; }
        public bool IsTree { get; set; }
        public bool IsLink { get; set; }
        public string TreeName { get; set; }
        public bool IsImage { get; set; }
        public bool IsText { get; set; }
        public bool IsMarkdown { get; set; }
        public string Path { get; set; }
        public byte[] Data { get; set; }
        public string Text { get; set; }
        public string TextBrush { get; set; }
        public Encoding Encoding { get; set; }
    }

    public class RepositoryTreeModel
    {
        public string Name { get; set; }
        public string Branch { get; set; }
        public string Path { get; set; }
        public string Readme { get; set; }
        public IEnumerable<RepositoryTreeDetailModel> Files { get; set; }
    }

    public class RepositoryCommitsModel
    {
        public string Name { get; set; }
        public IEnumerable<RepositoryCommitModel> Commits { get; set; }
    }

    public class RepositoryCommitChangeModel
    {
        public string ChangeId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public ChangeKind Status { get; set; }
        public int LinesAdded { get; set; }
        public int LinesDeleted { get; set; }
        public int LinesChanged { get { return LinesAdded + LinesDeleted; } }
        public string Patch { get; set; }
    }

    public class RepositoryCommitNoteModel
    {
        public RepositoryCommitNoteModel(string message, string @namespace)
        {
            this.Message = message;
            this.Namespace = @namespace;
        }

        public string Message { get; set; }

        public string Namespace { get; set; }
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

        [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_AuthorAvatar")]
        public string AuthorAvatar { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_Date")]
        public DateTime Date { get; set; }

        private string _message;
        [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_Message")]

        public string Message
        {
            get
            {
                if (String.IsNullOrEmpty(_message))
                {
                    return Resources.Repository_Commit_NoMessageDeclared;
                }
                else
                {
                    return _message;
                }
            }
            set
            {
                _message = value;
            }
        }

        public string MessageShort { get; set; }

        public string TagName { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_Changes")]
        public IEnumerable<RepositoryCommitChangeModel> Changes { get; set; }

        public IEnumerable<RepositoryCommitNoteModel> Notes { get; set; }
    }

    public class RepositoryBlameModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public long FileSize { get; set; }
        public long LineCount { get; set; }
        public IEnumerable<RepositoryBlameHunkModel> Hunks { get; set; }
    }

    public class RepositoryBlameHunkModel
    {
        public RepositoryCommitModel Commit { get; set; }
        public string[] Lines { get; set; }
    }

    public class RepositoryLogoDetailModel
    {
        byte[] _data;

        public RepositoryLogoDetailModel() { }

        public RepositoryLogoDetailModel(byte[] data)
        {
            this._data = data;
        }

        [FileUploadExtensions(Extensions = "PNG,JPG,JPEG,GIF")]
        [Display(ResourceType = typeof(Resources), Name = "Repository_Detail_Logo_PostedFile")]
        public HttpPostedFileWrapper PostedFile { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Repository_Detail_RemoveLogo")]
        public bool RemoveLogo { get; set; }

        public bool Exists
        {
            get
            {
                return BinaryData != null;
            }
        }

        public byte[] BinaryData
        {
            get
            {
                if (_data == null && PostedFile != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Image originalImage = Image.FromStream(PostedFile.InputStream, true, true);

                        Image resizedImage = originalImage.GetThumbnailImage(36, (36 * originalImage.Height) / originalImage.Width, null, IntPtr.Zero);

                        resizedImage.Save(ms, ImageFormat.Jpeg);

                        _data = ms.GetBuffer();
                    }
                }

                return _data;
            }
        }

        public string Base64Image
        {
            get
            {
                if (_data != null)
                    return Convert.ToBase64String(_data);

                return null;
            }
        }
    }
}