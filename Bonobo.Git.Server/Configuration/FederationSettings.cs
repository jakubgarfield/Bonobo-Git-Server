using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Configuration
{
    public class FederationSettings
    {
        public static string MetadataAddress { get; private set; }
        public static string Realm { get; private set; }

        static FederationSettings()
        {
            MetadataAddress = ConfigurationManager.AppSettings["FederationMetadataAddress"];
            Realm = ConfigurationManager.AppSettings["FederationRealm"];
        }
    }
}