namespace Bonobo.Git.Server.Configuration
{
    public class FederationSettings
    {
        public string MetadataAddress { get; private set; }
        public string Realm { get; private set; }

        static FederationSettings()
        {
            //MetadataAddress = ConfigurationManager.AppSettings["FederationMetadataAddress"];
            //Realm = ConfigurationManager.AppSettings["FederationRealm"];
        }
    }
}