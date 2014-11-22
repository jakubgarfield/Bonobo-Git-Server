using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Xml.Serialization;

namespace Bonobo.Git.Server.Configuration
{
    using Bonobo.Git.Server.App_GlobalResources;

    [XmlRootAttribute(ElementName = "Configuration", IsNullable = false)]
    public class UserConfiguration : ConfigurationEntry<UserConfiguration>
    {      
        public bool AllowAnonymousPush { get; set; }
        public string Repositories { get; set; }
        public bool AllowUserRepositoryCreation { get; set; }
        public bool AllowAnonymousRegistration { get; set; }
        public string DefaultLanguage { get; set; }
        public string SiteTitle { get; set; }
        public string SiteLogoUrl { get; set; }
        public string SiteFooterMessage { get; set; }
        public bool IsCommitAuthorAvatarVisible { get; set; }

        public bool HasSiteFooterMessage
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.SiteFooterMessage);
            }
        }

        public bool HasCustomSiteLogo
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.SiteLogoUrl);
            }
        }

        public string GetSiteTitle()
        {
            return !string.IsNullOrWhiteSpace(this.SiteTitle) ? this.SiteTitle : Resources.Layout_Title;
        }

        public static void Initialize()
        {
            if (IsInitialized())
                return;

            Current.Repositories = Path.IsPathRooted(ConfigurationManager.AppSettings["DefaultRepositoriesDirectory"]) 
                ? ConfigurationManager.AppSettings["DefaultRepositoriesDirectory"] 
                : HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["DefaultRepositoriesDirectory"]);
            Current.Save();
        }


        private static bool IsInitialized()
        {
            return !String.IsNullOrEmpty(Current.Repositories);
        }
    }
}