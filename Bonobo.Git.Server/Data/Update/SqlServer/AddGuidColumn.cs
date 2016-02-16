namespace Bonobo.Git.Server.Data.Update.SqlServer
{
    public class AddGuidColumn : IUpdateScript
    {
        public string Command
        {
            get
            {
                return "ALTER TABLE Repository ADD COLUMN [Group] VARCHAR(255) DEFAULT(NULL)";
            }
        }

        public string Precondition
        {
            get
            {
                return "SELECT Count([Group]) = -1 FROM Repository";
            }
        }
    }
}
