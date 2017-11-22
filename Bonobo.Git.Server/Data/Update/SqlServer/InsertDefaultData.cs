using System;

namespace Bonobo.Git.Server.Data.Update.SqlServer
{
    public class InsertDefaultData : IUpdateScript
    {
        public string Command
        {
            get
            {
                Guid roleId = new Guid("a3139d2b-5a59-427f-bb2d-af251dce00e4");
                Guid UserId = new Guid("3eb9995e-99e3-425a-b978-1409bdd61fb6");
                return @"

                    INSERT INTO [Role] ([Id], [Name], [Description]) VALUES ('" + roleId + @"','Administrator','System administrator');
                    INSERT INTO [User] ([Id], [Name], [Surname], [Username], [Password], [PasswordSalt], [Email]) VALUES ('" + UserId + @"','admin', '', 'admin', '0CC52C6751CC92916C138D8D714F003486BF8516933815DFC11D6C3E36894BFA044F97651E1F3EEBA26CDA928FB32DE0869F6ACFB787D5A33DACBA76D34473A3', 'admin', '');
                    INSERT INTO [UserRole_InRole] ([User_Id], [Role_Id]) VALUES ('" + UserId + "','" + roleId + @"');
                    ";
            }
        }

        public string Precondition
        {
            get { return @"SELECT case when count(*) = 0 then 1 ELSE 0 END FROM [User]"; }
        }

        public void CodeAction(BonoboGitServerContext context) { }
    }
}