using System;

namespace Bonobo.Git.Server.Data.Update.Sqlite
{
    public class AddAuditPushUser : IUpdateScript
    {
        public string Command
        {
            get
            {
                return @"
                    
                ALTER TABLE Repository ADD COLUMN AuditPushUser BIT NOT NULL DEFAULT('0')
";
            }
        }

        public string Precondition
        {
            get
            {
                return "SELECT Count(AuditPushUser) = -1 FROM [Repository]";
            }
        }

        public void CodeAction(BonoboGitServerContext context) {}

    }
}