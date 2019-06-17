namespace Bonobo.Git.Server.Data.Update.Data
{
    public class OldRepo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Anonymous { get; set; }
        public bool AuditPushUser { get; set; }
        public string Group { get; set; }
        public byte[] Logo { get; set; }
    }
}
