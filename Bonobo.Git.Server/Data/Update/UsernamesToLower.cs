namespace Bonobo.Git.Server.Data.Update
{
    public class UsernamesToLower : IUpdateScript
    {
        public string Command
        {
            get 
            {
                return @"
                    UPDATE [User] SET Username = lower(Username);
                    UPDATE [UserRepository_Administrator] SET User_Username = lower(User_Username);
                    UPDATE [UserRepository_Permission] SET User_Username = lower(User_Username);
                    UPDATE [UserRole_InRole] SET User_Username = lower(User_Username);
                    UPDATE [UserTeam_Member] SET User_Username = lower(User_Username);
                ";
            }
        }

        public string Precondition
        {
            get { return null; }
        }
    }
}