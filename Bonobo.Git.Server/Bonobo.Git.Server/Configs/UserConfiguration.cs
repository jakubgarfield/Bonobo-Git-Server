using System.Xml.Serialization;

namespace Bonobo.Git.Server.Configs
{
    [XmlRootAttribute(ElementName = "Configuration", IsNullable = false)]
    public class UserConfiguration : ConfigEntry<UserConfiguration>
    {
        public bool AllowAnonymousPush { get; set; }
        public string Repositories { get; set; }
        public bool AllowUserRepositoryCreation { get; set; }
        public bool AllowAnonymousRegistration { get; set; }
    }
}