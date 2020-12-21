using Bonobo.Git.Server.App_GlobalResources;
using System;
using System.Configuration;
using System.Xml.Serialization;

namespace Bonobo.Git.Server.Configuration
{

    [XmlRoot(ElementName = "Configuration", IsNullable = false)]
    public class UserConfiguration : ConfigurationEntry<UserConfiguration>
    {
        public bool AllowAnonymousPush { get; set; }
        [XmlElement(ElementName = "Repositories")]
        public string RepositoryPath { get; set; }
        public bool AllowUserRepositoryCreation { get; set; }
        public bool AllowPushToCreate { get; set; }
        public bool AllowAnonymousRegistration { get; set; }
        public string DefaultLanguage { get; set; }
        public string SiteTitle { get; set; }
        public string SiteLogoUrl { get; set; }
        public string SiteFooterMessage { get; set; }
        public string SiteCssUrl { get; set; }
        public bool IsCommitAuthorAvatarVisible { get; set; }
        public string LinksRegex { get; set; }
        public string LinksUrl { get; set; }

        public string Repositories => PathResolver.Resolve(RepositoryPath);

        public bool HasSiteFooterMessage => !string.IsNullOrWhiteSpace(this.SiteFooterMessage);

        public bool HasCustomSiteLogo => !string.IsNullOrWhiteSpace(this.SiteLogoUrl);

        public bool HasCustomSiteCss => !string.IsNullOrWhiteSpace(SiteCssUrl);

        public bool HasLinks => !string.IsNullOrWhiteSpace(this.LinksRegex);

        public string GetSiteTitle() => !string.IsNullOrWhiteSpace(this.SiteTitle) ? this.SiteTitle : Resources.Layout_Title;

        public static void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            Current.RepositoryPath = ConfigurationManager.AppSettings["DefaultRepositoriesDirectory"];
            Current.Save();
        }

        private static bool IsInitialized => !String.IsNullOrEmpty(Current.RepositoryPath);
    }
}