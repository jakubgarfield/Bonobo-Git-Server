using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Bonobo.Git.Server
{
    [XmlRootAttribute(ElementName = "Configuration", IsNullable = false)]
    public class UserConfiguration
    {
        public bool AllowAnonymousPush { get; set; }
        public string Repositories { get; set; }
    }
}