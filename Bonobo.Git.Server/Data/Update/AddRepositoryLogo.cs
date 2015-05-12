namespace Bonobo.Git.Server.Data.Update
{
    public class AddRepositoryLogo : IUpdateScript
    {
        public string Command
        {
            get
            {
                return "ALTER TABLE Repository ADD COLUMN [Logo] Blob DEFAULT(NULL)";
            }
        }

        public string Precondition
        {
            get
            {
                return "SELECT Count([Logo]) = -1 FROM Repository";
            }
        }
    }
}