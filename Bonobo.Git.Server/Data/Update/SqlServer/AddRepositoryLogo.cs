namespace Bonobo.Git.Server.Data.Update.SqlServer
{
    public class AddRepositoryLogo : IUpdateScript
    {
        public string Command
        {
            get
            {
                return "ALTER TABLE Repository ADD [Logo] [varbinary](max) NULL";
            }
        }

        public string Precondition
        {
            get
            {
                return @"
            IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Repository' AND  COLUMN_NAME = 'Logo')
                SELECT 0
            ELSE
                SELECT 1
";
            }
        }
    }
}