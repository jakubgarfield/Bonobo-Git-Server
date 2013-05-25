using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Data.Update
{
    public class InsertDefaultData : IUpdateScript
    {
        public string Command
        {
            get
            {
                return @"

                    INSERT INTO [Role] ([Name], [Description]) VALUES ('Administrator','System administrator');
                    INSERT INTO [User] ([Name], [Surname], [Username], [Password], [Email]) VALUES ('admin', '', 'admin', '21232F297A57A5A743894A0E4A801FC3', '');
                    INSERT INTO [UserRole_InRole] ([User_Username], [Role_Name]) VALUES ('admin', 'Administrator');

                    ";
            }
        }

        public string Precondition
        {
            get { return @"SELECT Count(*) = 0 FROM [User]"; }
        }
    }
}