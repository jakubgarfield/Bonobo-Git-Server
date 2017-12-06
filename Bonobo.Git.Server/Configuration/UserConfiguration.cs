using System;
using System.Xml.Serialization;

namespace Bonobo.Git.Server.Configuration
{
    using Bonobo.Git.Server.App_GlobalResources;

    [XmlRootAttribute(ElementName = "Configuration", IsNullable = false)]
    public class UserConfiguration : ConfigurationEntry<UserConfiguration>
    {
        public bool AllowAnonymousPush { get; set; }
        [XmlElementAttribute(ElementName = "Repositories")]
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

        public string Repositories
        {
            get
            {
                return RepositoryPath ?? ".";
                //return Path.IsPathRooted(RepositoryPath)
                //       ? RepositoryPath
                //       : HostingEnvironment.MapPath(RepositoryPath);
            }
        }

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

        public bool HasCustomSiteCss
        {
            get { return !string.IsNullOrWhiteSpace(SiteCssUrl); }
        }

        public bool HasLinks
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.LinksRegex);
            }
        }

        public string GetSiteTitle()
        {
            return !string.IsNullOrWhiteSpace(this.SiteTitle) ? this.SiteTitle : Resources.Layout_Title;
        }

        public static void Initialize(AppSettings appSettings)
        {
            if (IsInitialized())
                return;

            System.Diagnostics.Debugger.Break();
            Current.RepositoryPath = appSettings.DefaultRepositoriesDirectory;
            Current.Save();
        }


        private static bool IsInitialized()
        {
            return !String.IsNullOrEmpty(Current.RepositoryPath);
        }
    }
}