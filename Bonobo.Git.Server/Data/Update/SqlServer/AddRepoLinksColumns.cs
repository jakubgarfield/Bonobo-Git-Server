namespace Bonobo.Git.Server.Data.Update.SqlServer
{
    public class AddRepoLinksColumn : IUpdateScript
    {
        public string Command
        {
            get
            {
                return @"ALTER TABLE Repository ADD [LinksRegex] NVARCHAR(255) Not Null CONSTRAINT lr_def DEFAULT '';
                         ALTER TABLE Repository ADD [LinksUrl] NVARCHAR(255) Not Null CONSTRAINT lu_def DEFAULT '';
                         ALTER TABLE Repository ADD [LinksUseGlobal] Bit Not Null CONSTRAINT lug_def DEFAULT 1;";
            }
        }

        public string Precondition
        {
            get
            {
                return @"
            IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Repository' AND  COLUMN_NAME = 'LinksRegex')
                SELECT 0
            ELSE
                SELECT 1
";
            }
        }

        public void CodeAction(BonoboGitServerContext context) { }

    }
}
