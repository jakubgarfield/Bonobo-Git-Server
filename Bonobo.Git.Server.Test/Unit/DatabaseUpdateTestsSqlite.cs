using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bonobo.Git.Server.Test.MembershipTests.EFTests;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Data;

namespace Bonobo.Git.Server.Test.Unit
{
    [TestClass]
    public class DatabaseUpdateTestsSqlite
    {
        protected IDatabaseTestConnection _connection;

        [TestInitialize]
        public void InitialiseTestObjects()
        {
            _connection = new SqliteTestConnection();
        }

        [TestMethod]
        public void RunUpdateOn_v1_2_0_25ddf80()
        {
            _connection.GetContext().Database.ExecuteSqlCommand(@"

                    CREATE TABLE IF NOT EXISTS [Repository] (
                        [Name] VarChar(255) Not Null,
                        [Description] VarChar(255) Null,
                        [Anonymous] Bit Not Null,
                        Constraint [PK_Repository] Primary Key ([Name])
                    );

                    CREATE TABLE IF NOT EXISTS [Role] (
                        [Name] VarChar(255) Not Null,
                        [Description] VarChar(255) Null,
                        Constraint [PK_Role] Primary Key ([Name])
                    );

                    CREATE TABLE IF NOT EXISTS [Team] (
                        [Name] VarChar(255) Not Null,
                        [Description] VarChar(255) Null,
                        Constraint [PK_Team] Primary Key ([Name])
                    );

                    CREATE TABLE IF NOT EXISTS [User] (
                        [Name] VarChar(255) Not Null,
                        [Surname] VarChar(255) Not Null,
                        [Username] VarChar(255) Not Null,
                        [Password] VarChar(255) Not Null,
                        [Email] VarChar(255) Not Null,
                        Constraint [PK_User] Primary Key ([Username])
                    );

                    CREATE TABLE IF NOT EXISTS [TeamRepository_Permission] (
                        [Team_Name] VarChar(255) Not Null,
                        [Repository_Name] VarChar(255) Not Null,
                        Constraint [UNQ_TeamRepository_Permission_1] Unique ([Team_Name], [Repository_Name]),
                        Foreign Key ([Team_Name]) References [Team]([Name]),
                        Foreign Key ([Repository_Name]) References [Repository]([Name])
                    );

                    CREATE TABLE IF NOT EXISTS [UserRepository_Administrator] (
                        [User_Username] VarChar(255) Not Null,
                        [Repository_Name] VarChar(255) Not Null,
                        Constraint [UNQ_UserRepository_Administrator_1] Unique ([User_Username], [Repository_Name]),
                        Foreign Key ([User_Username]) References [User]([Username]),
                        Foreign Key ([Repository_Name]) References [Repository]([Name])
                    );

                    CREATE TABLE IF NOT EXISTS [UserRepository_Permission] (
                        [User_Username] VarChar(255) Not Null,
                        [Repository_Name] VarChar(255) Not Null,
                        Constraint [UNQ_UserRepository_Permission_1] Unique ([User_Username], [Repository_Name]),
                        Foreign Key ([User_Username]) References [User]([Username]),
                        Foreign Key ([Repository_Name]) References [Repository]([Name])
                    );

                    CREATE TABLE IF NOT EXISTS [UserRole_InRole] (
                        [User_Username] VarChar(255) Not Null,
                        [Role_Name] VarChar(255) Not Null,
                        Constraint [UNQ_UserRole_InRole_1] Unique ([User_Username], [Role_Name]),
                        Foreign Key ([User_Username]) References [User]([Username]),
                        Foreign Key ([Role_Name]) References [Role]([Name])
                    );

                    CREATE TABLE IF NOT EXISTS [UserTeam_Member] (
                        [User_Username] VarChar(255) Not Null,
                        [Team_Name] VarChar(255) Not Null,
                        Constraint [UNQ_UserTeam_Member_1] Unique ([User_Username], [Team_Name]),
                        Foreign Key ([User_Username]) References [User]([Username]),
                        Foreign Key ([Team_Name]) References [Team]([Name])
                    );
                  
                    ");
            new AutomaticUpdater().RunWithContext(_connection.GetContext());
        }

        [TestMethod]
        public void RunUpdateOn_6_0_0_0861955()
        {
            _connection.GetContext().Database.ExecuteSqlCommand(
                string.Format(@"

                    CREATE TABLE IF NOT EXISTS [Repository] (
                        [Id] Char(36) PRIMARY KEY NOT NULL,
                        [Name] VarChar(255) Not Null UNIQUE,
                        [Description] VarChar(255) Null,
                        [Anonymous] Bit Not Null,
                        [AllowAnonymousPush] Integer NULL Default {0},
                        [LinksRegex] VarChar(255) Not Null,
                        [LinksUrl] VarChar(255) Not Null,
                        [LinksUseGlobal] Bit default 1 Not Null,
                        UNIQUE ([Name] COLLATE NOCASE)
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

                    ", (int)RepositoryPushMode.Global)
            );
        }
    }
}
