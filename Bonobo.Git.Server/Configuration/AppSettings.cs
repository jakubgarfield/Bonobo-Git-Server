namespace Bonobo.Git.Server.Configuration
{
    public class AppSettings
    {
        public bool IsPushAuditEnabled { get; set; }
        public string LogDirectory { get; set; }
        public string RecoveryDataPath { get; set; }
        public string GitPath { get; set; }
        public string GitHomePath { get; set; }
        public string GitServerPath { get; set; }
        public bool AllowDBReset { get; set; }
        public string DefaultRepositoriesDirectory { get; set; }
        public string ActiveDirectoryDefaultDomain { get; set; }
    }
}