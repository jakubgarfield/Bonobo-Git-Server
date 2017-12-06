namespace Bonobo.Git.Server.Data.Update.Sqlite
{
    public class AddRepoLinksColumn : IUpdateScript
    {
        public string Command
        {
            get
            {
                return @"ALTER TABLE Repository ADD COLUMN [LinksRegex] VARCHAR(255) Not Null DEFAULT('');
                         ALTER TABLE Repository ADD COLUMN [LinksUrl] VARCHAR(255) Not Null DEFAULT('');
                         ALTER TABLE Repository ADD COLUMN [LinksUseGlobal] INT DEFAULT(1);";
            }
        }

        public string Precondition
        {
            get
            {
                return "SELECT Count([LinksRegex]) = -1 FROM Repository";
            }
        }

        public void CodeAction(BonoboGitServerContext context) { }

    }
}
