using System.Collections.Generic;
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
        string GenerateResetToken(string username);
    }
}