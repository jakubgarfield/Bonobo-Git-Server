using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bonobo.Git.Server.Security;
using Bonobo.Git.Server.Data;

namespace Bonobo.Git.Server.Test
{
    [TestClass]
    public class EFMembershipServiceTest
    {
        string connString = "metadata=res://*/Data.DatabaseModel.csdl|res://*/Data.DatabaseModel.ssdl|res://*/Data.DatabaseModel.msl;provider=System.Data.SQLite;provider connection string='data source=&quot;D:\\Projects\\Bonobo Git Server\\Source\\Bonobo.Git.Server\\Bonobo.Git.Server\\App_Data\\Bonobo.Git.Server.db&quot;'";
        [TestMethod]
        public void CreateUser_Test()
        {            
            CreateUser("test", "aaa", "johny", "test", "a@a.cz");
        }

        public bool CreateUser(string username, string password, string name, string surname, string email)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", "name");
            if (String.IsNullOrEmpty(surname)) throw new ArgumentException("Value cannot be null or empty.", "surname");
            if (String.IsNullOrEmpty(email)) throw new ArgumentException("Value cannot be null or empty.", "email");

            using (var database = new DataEntities(connString))
            {
                var user = new User
                {
                    Username = username,
                    Password = password,
                    Name = name,
                    Surname = surname,
                    Email = email,
                };
                database.AddToUser(user);
                database.SaveChanges();
            }
            return true;
        }
    }
}
