using System.Collections.Generic;
using Bonobo.Git.Server.Models;
using System;

namespace Bonobo.Git.Server.Security
{
    public interface IMembershipService
    {
        bool IsReadOnly();
        ValidationResult ValidateUser(string username, string password);
        bool CreateUser(string username, string password, string name, string surname, string email);
        IList<UserModel> GetAllUsers();
        UserModel GetUserModel(Guid id);
        UserModel GetUserModel(string username);
        void UpdateUser(Guid id, string username, string name, string surname, string email, string password);
        void DeleteUser(Guid id);
        string GenerateResetToken(string username);
    }
}