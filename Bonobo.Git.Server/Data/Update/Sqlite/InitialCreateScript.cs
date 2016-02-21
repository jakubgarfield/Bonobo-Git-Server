
namespace Bonobo.Git.Server.Data.Update.Sqlite
{
    public class InitialCreateScript : IUpdateScript
    {
        public string Command
        {
            get 
            {
                return @"

                    CREATE TABLE IF NOT EXISTS [Repository] (
                        [Id] Char(36) PRIMARY KEY NOT NULL,
                        [Name] VarChar(255) Not Null UNIQUE,
                        [Description] VarChar(255) Null,
                        [Anonymous] Bit Not Null
                    );

                    CREATE TABLE IF NOT EXISTS [Role] (
                        [Id] Char(36) PRIMARY KEY,
                        [Name] VarChar(255) Not Null UNIQUE,
                        [Description] VarChar(255) Null
                    );

                    CREATE TABLE IF NOT EXISTS [Team] (
                        [Id] Char(36) PRIMARY KEY,
                        [Name] VarChar(255) Not Null UNIQUE,
                        [Description] VarChar(255) Null
                    );

                    CREATE TABLE IF NOT EXISTS [User] (
                        [Id] Char(36) PRIMARY KEY,
                        [Name] VarChar(255) Not Null,
                        [Surname] VarChar(255) Not Null,
                        [Username] VarChar(255) Not Null UNIQUE,
                        [Password] VarChar(255) Not Null,
                        [PasswordSalt] VarChar(255) Not Null,
                        [Email] VarChar(255) Not Null
                    );

                    CREATE TABLE IF NOT EXISTS [TeamRepository_Permission] (
                        [Team_Id] Char(36) Not Null,
                        [Repository_Id] Char(36) Not Null,
                        Constraint [UNQ_TeamRepository_Permission_1] Unique ([Team_Id], [Repository_Id]),
                        Foreign Key ([Team_Id]) References [Team]([Id]),
                        Foreign Key ([Repository_Id]) References [Repository]([Id])
                    );

                    CREATE TABLE IF NOT EXISTS [UserRepository_Administrator] (
                        [User_Id] Char(36) Not Null,
                        [Repository_Id] Char(36) Not Null,
                        Constraint [UNQ_UserRepository_Administrator_1] Unique ([User_Id], [Repository_Id]),
                        Foreign Key ([User_Id]) References [User]([Id]),
                        Foreign Key ([Repository_Id]) References [Repository]([Id])
                    );

                    CREATE TABLE IF NOT EXISTS [UserRepository_Permission] (
                        [User_Id] Char(36) Not Null,
                        [Repository_Id] Char(36) Not Null,
                        Constraint [UNQ_UserRepository_Permission_1] Unique ([User_Id], [Repository_Id]),
                        Foreign Key ([User_Id]) References [User]([Id]),
                        Foreign Key ([Repository_Id]) References [Repository]([Id])
                    );

                    CREATE TABLE IF NOT EXISTS [UserRole_InRole] (
                        [User_Id] Char(36) Not Null,
                        [Role_Id] Char(36) Not Null,
                        Constraint [UNQ_UserRole_InRole_1] Unique ([User_Id], [Role_Id]),
                        Foreign Key ([User_Id]) References [User]([Id]),
                        Foreign Key ([Role_Id]) References [Role]([Id])
                    );

                    CREATE TABLE IF NOT EXISTS [UserTeam_Member] (
                        [User_Id] Char(36) Not Null,
                        [Team_Id] Char(36) Not Null,
                        Constraint [UNQ_UserTeam_Member_1] Unique ([User_Id], [Team_Id]),
                        Foreign Key ([User_Id]) References [User]([Id]),
                        Foreign Key ([Team_Id]) References [Team]([Id])
                    );

                    ";
            }
        }

        public string Precondition
        {
            get { return null; }
        }
    }
}