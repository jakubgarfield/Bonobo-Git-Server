namespace Bonobo.Git.Server.Data.Update.SqlServer
{
    public class InsertDefaultData : IUpdateScript
    {
        public string Command
        {
            get
            {
                return @"

                    INSERT INTO [Role] ([Name], [Description]) VALUES ('Administrator','System administrator');
                    INSERT INTO [User] ([Name], [Surname], [Username], [Password], [Email]) VALUES ('admin', '', 'admin', '0CC52C6751CC92916C138D8D714F003486BF8516933815DFC11D6C3E36894BFA044F97651E1F3EEBA26CDA928FB32DE0869F6ACFB787D5A33DACBA76D34473A3', '');
                    INSERT INTO [UserRole_InRole] ([User_Username], [Role_Name]) VALUES ('admin', 'Administrator');

                    ";
            }
        }

        public string Precondition
        {
            get { return @"SELECT case when count(*) = 0 then 1 ELSE 0 END FROM [User]"; }
        }
    }
}