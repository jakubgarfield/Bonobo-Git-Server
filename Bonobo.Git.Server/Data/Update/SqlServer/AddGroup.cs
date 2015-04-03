namespace Bonobo.Git.Server.Data.Update.SqlServer
{
    public class AddGroup : IUpdateScript
    {
        public string Command
        {
            get
            {
                return "ALTER TABLE Repository ADD [Group] NVARCHAR(255) NULL";
            }
        }

        public string Precondition
        {
            get
            {
                return @"
            IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Repository' AND  COLUMN_NAME = 'Group')
                SELECT 0
            ELSE
                SELECT 1
";
            }
        }
    }
}