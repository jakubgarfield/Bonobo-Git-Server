using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Xml.Serialization;

namespace Bonobo.Git.Server.Configuration
{
    [XmlRootAttribute(ElementName = "Configuration", IsNullable = false)]
    public class UserConfiguration : ConfigurationEntry<UserConfiguration>
    {      
        public bool AllowAnonymousPush { get; set; }
        public string Repositories { get; set; }
        public bool AllowUserRepositoryCreation { get; set; }
        public bool AllowAnonymousRegistration { get; set; }


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