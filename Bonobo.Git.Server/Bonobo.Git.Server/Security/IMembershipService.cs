using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Security
{
    public interface IMembershipService
    {
        bool ValidateUser(string username, string password);
        bool CreateUser(string username, string password, string name, string surname, string email);
        IList<UserModel> GetAllUsers();
        UserModel GetUser(string username);
        void UpdateUser(string username, string name, string surname, string email, string password);
        void DeleteUser(string username);
    }
}