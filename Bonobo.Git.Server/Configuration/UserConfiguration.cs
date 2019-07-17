using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Xml.Serialization;

namespace Bonobo.Git.Server.Configuration
{
    using Bonobo.Git.Server.App_GlobalResources;
    using System.Web.Hosting;

    [XmlRootAttribute(ElementName = "Configuration", IsNullable = false)]
    public class UserConfiguration : ConfigurationEntry<UserConfiguration>
    {
        private static bool _initialized = false;

        public bool AllowAnonymousPush { get; set; }
        [XmlElementAttribute(ElementName = "Repositories")]
        public string RepositoryPath { get; set; }
        [XmlElementAttribute(ElementName = "LfsPath")]
        public string LfsPath { get; set; }
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
                return Path.IsPathRooted(RepositoryPath)
                       ? RepositoryPath
                       : HostingEnvironment.MapPath(RepositoryPath);
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

        public static void Initialize()
        {
            if (!_initialized)
            {
                // This is a bit of a cheat.  It loads from web.config whereas nothing else does.
                // It will be overridden with a value derived from the Repositories setting in the 
                // config.xml if it is missing from web.config, so it is optional.
                Current.LfsPath = ConfigurationManager.AppSettings["DefaultLfsDirectory"];
                Current.Save();
                _initialized = true;
            }

            // Because this triggers an initialization it will never return false...
            if (IsInitialized())
                return;
        }


        private static bool IsInitialized()
        {   
            // Bug: This triggers an initial config load and therefore always returns true.
            return !String.IsNullOrEmpty(Current.RepositoryPath);
        }
    }
}