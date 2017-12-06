namespace Bonobo.Git.Server.Data.Update.SqlServer
{
    public class AddAuditPushUser : IUpdateScript
    {
        public string Command
        {
            get
            {
                return @"
                    
                ALTER TABLE Repository ADD AuditPushUser BIT NOT NULL DEFAULT('0')
";
            }
        }

        public string Precondition
        {
            get
            {
                return @"
                
            IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Repository' AND  COLUMN_NAME = 'AuditPushUser')
                SELECT 0
            ELSE
                SELECT 1

";
            }
        }

        public void CodeAction(BonoboGitServerContext context) { }
    }
}