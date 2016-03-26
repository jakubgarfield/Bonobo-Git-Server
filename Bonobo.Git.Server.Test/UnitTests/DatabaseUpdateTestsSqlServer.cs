using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bonobo.Git.Server.Test.MembershipTests.EFTests;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Data;

namespace Bonobo.Git.Server.Test.UnitTests
{
    [TestClass]
    public class DatabaseUpdateTestsSqlServer
    {
        protected IDatabaseTestConnection _connection;

        [TestInitialize]
        public void InitialiseTestObjects()
        {
            _connection = new SqlServerTestConnection();
        }

        // There is not test for version 1.2.0 because SqlServer was first introduced in 2.1
        [TestMethod]
        public void RunUpdateOn_2_1_96fa2a()
        {
            _connection.GetContext().Database.ExecuteSqlCommand(@"
                IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Repository'))
                    BEGIN
                        CREATE TABLE [Repository] (
                            [Name] VarChar(255) Not Null,
                            [Description] VarChar(255) Null,
                            [Anonymous] Bit Not Null,
                            Constraint [PK_Repository] Primary Key ([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Role'))
                    BEGIN
                        CREATE TABLE [Role] (
                            [Name] VarChar(255) Not Null,
                            [Description] VarChar(255) Null,
                            Constraint [PK_Role] Primary Key ([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Team'))
                    BEGIN
                        CREATE TABLE [Team] (
                            [Name] VarChar(255) Not Null,
                            [Description] VarChar(255) Null,
                            Constraint [PK_Team] Primary Key ([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'User'))
                    BEGIN
                        CREATE TABLE [User] (
                            [Name] VarChar(255) Not Null,
                            [Surname] VarChar(255) Not Null,
                            [Username] VarChar(255) Not Null,
                            [Password] VarChar(255) Not Null,
                            [Email] VarChar(255) Not Null,
                            Constraint [PK_User] Primary Key ([Username])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'TeamRepository_Permission'))
                    BEGIN
                        CREATE TABLE [TeamRepository_Permission] (
                            [Team_Name] VarChar(255) Not Null,
                            [Repository_Name] VarChar(255) Not Null,
                            Constraint [UNQ_TeamRepository_Permission_1] Unique ([Team_Name], [Repository_Name]),
                            Foreign Key ([Team_Name]) References [Team]([Name]),
                            Foreign Key ([Repository_Name]) References [Repository]([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'UserRepository_Administrator'))
                    BEGIN
                        CREATE TABLE [UserRepository_Administrator] (
                            [User_Username] VarChar(255) Not Null,
                            [Repository_Name] VarChar(255) Not Null,
                            Constraint [UNQ_UserRepository_Administrator_1] Unique ([User_Username], [Repository_Name]),
                            Foreign Key ([User_Username]) References [User]([Username]),
                            Foreign Key ([Repository_Name]) References [Repository]([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'UserRepository_Permission'))
                    BEGIN
                        CREATE TABLE [UserRepository_Permission] (
                            [User_Username] VarChar(255) Not Null,
                            [Repository_Name] VarChar(255) Not Null,
                            Constraint [UNQ_UserRepository_Permission_1] Unique ([User_Username], [Repository_Name]),
                            Foreign Key ([User_Username]) References [User]([Username]),
                            Foreign Key ([Repository_Name]) References [Repository]([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'UserRole_InRole'))
                    BEGIN
                        CREATE TABLE [UserRole_InRole] (
                            [User_Username] VarChar(255) Not Null,
                            [Role_Name] VarChar(255) Not Null,
                            Constraint [UNQ_UserRole_InRole_1] Unique ([User_Username], [Role_Name]),
                            Foreign Key ([User_Username]) References [User]([Username]),
                            Foreign Key ([Role_Name]) References [Role]([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'UserTeam_Member'))
                    BEGIN
                        CREATE TABLE [UserTeam_Member] (
                            [User_Username] VarChar(255) Not Null,
                            [Team_Name] VarChar(255) Not Null,
                            Constraint [UNQ_UserTeam_Member_1] Unique ([User_Username], [Team_Name]),
                            Foreign Key ([User_Username]) References [User]([Username]),
                            Foreign Key ([Team_Name]) References [Team]([Name])
                        );
                    END
                ");
        }

        [TestMethod]
        public void RunUpdateOn_3_3_0_c6e0c6()
        {
            _connection.GetContext().Database.ExecuteSqlCommand(@"
                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Repository'))
                    BEGIN
                        CREATE TABLE [dbo].[Repository] (
                            [Name] VarChar(255) Not Null,
                            [Description] VarChar(255) Null,
                            [Anonymous] Bit Not Null,
                            Constraint [PK_Repository] Primary Key ([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Role'))
                    BEGIN
                        CREATE TABLE [dbo].[Role] (
                            [Name] VarChar(255) Not Null,
                            [Description] VarChar(255) Null,
                            Constraint [PK_Role] Primary Key ([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Team'))
                    BEGIN
                        CREATE TABLE [dbo].[Team] (
                            [Name] VarChar(255) Not Null,
                            [Description] VarChar(255) Null,
                            Constraint [PK_Team] Primary Key ([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'User'))
                    BEGIN
                        CREATE TABLE [dbo].[User] (
                            [Name] VarChar(255) Not Null,
                            [Surname] VarChar(255) Not Null,
                            [Username] VarChar(255) Not Null,
                            [Password] VarChar(255) Not Null,
                            [Email] VarChar(255) Not Null,
                            Constraint [PK_User] Primary Key ([Username])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'TeamRepository_Permission'))
                    BEGIN
                        CREATE TABLE [dbo].[TeamRepository_Permission] (
                            [Team_Name] VarChar(255) Not Null,
                            [Repository_Name] VarChar(255) Not Null,
                            Constraint [UNQ_TeamRepository_Permission_1] Unique ([Team_Name], [Repository_Name]),
                            Foreign Key ([Team_Name]) References [Team]([Name]),
                            Foreign Key ([Repository_Name]) References [Repository]([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'UserRepository_Administrator'))
                    BEGIN
                        CREATE TABLE [dbo].[UserRepository_Administrator] (
                            [User_Username] VarChar(255) Not Null,
                            [Repository_Name] VarChar(255) Not Null,
                            Constraint [UNQ_UserRepository_Administrator_1] Unique ([User_Username], [Repository_Name]),
                            Foreign Key ([User_Username]) References [User]([Username]),
                            Foreign Key ([Repository_Name]) References [Repository]([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'UserRepository_Permission'))
                    BEGIN
                        CREATE TABLE [dbo].[UserRepository_Permission] (
                            [User_Username] VarChar(255) Not Null,
                            [Repository_Name] VarChar(255) Not Null,
                            Constraint [UNQ_UserRepository_Permission_1] Unique ([User_Username], [Repository_Name]),
                            Foreign Key ([User_Username]) References [User]([Username]),
                            Foreign Key ([Repository_Name]) References [Repository]([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'UserRole_InRole'))
                    BEGIN
                        CREATE TABLE [dbo].[UserRole_InRole] (
                            [User_Username] VarChar(255) Not Null,
                            [Role_Name] VarChar(255) Not Null,
                            Constraint [UNQ_UserRole_InRole_1] Unique ([User_Username], [Role_Name]),
                            Foreign Key ([User_Username]) References [User]([Username]),
                            Foreign Key ([Role_Name]) References [Role]([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'UserTeam_Member'))
                    BEGIN
                        CREATE TABLE [dbo].[UserTeam_Member] (
                            [User_Username] VarChar(255) Not Null,
                            [Team_Name] VarChar(255) Not Null,
                            Constraint [UNQ_UserTeam_Member_1] Unique ([User_Username], [Team_Name]),
                            Foreign Key ([User_Username]) References [User]([Username]),
                            Foreign Key ([Team_Name]) References [Team]([Name])
                        );
                    END
            ");
        }

        [TestMethod]
        public void RunUpdateOn_6_0_0_0861955()
        {
            _connection.GetContext().Database.ExecuteSqlCommand(
                string.Format(@"
                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Repository'))
                    BEGIN
                        CREATE TABLE [dbo].[Repository] (
                            [Name] VarChar(255) Not Null,
                            [Description] VarChar(255) Null,
                            [Anonymous] Bit Not Null,
                            [AllowAnonymousPush] Integer Default {0} Not Null,
                            [LinksRegex] VarChar(255) Not Null,
                            [LinksUrl] VarChar(255) Not Null,
                            [LinksUseGlobal] Bit Default 1 Not Null,
                            Constraint [PK_Repository] Primary Key ([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Role'))
                    BEGIN
                        CREATE TABLE [dbo].[Role] (
                            [Name] VarChar(255) Not Null,
                            [Description] VarChar(255) Null,
                            Constraint [PK_Role] Primary Key ([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Team'))
                    BEGIN
                        CREATE TABLE [dbo].[Team] (
                            [Name] VarChar(255) Not Null,
                            [Description] VarChar(255) Null,
                            Constraint [PK_Team] Primary Key ([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'User'))
                    BEGIN
                        CREATE TABLE [dbo].[User] (
                            [Name] VarChar(255) Not Null,
                            [Surname] VarChar(255) Not Null,
                            [Username] VarChar(255) Not Null,
                            [Password] VarChar(255) Not Null,
                            [Email] VarChar(255) Not Null,
                            Constraint [PK_User] Primary Key ([Username])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'TeamRepository_Permission'))
                    BEGIN
                        CREATE TABLE [dbo].[TeamRepository_Permission] (
                            [Team_Name] VarChar(255) Not Null,
                            [Repository_Name] VarChar(255) Not Null,
                            Constraint [UNQ_TeamRepository_Permission_1] Unique ([Team_Name], [Repository_Name]),
                            Foreign Key ([Team_Name]) References [Team]([Name]),
                            Foreign Key ([Repository_Name]) References [Repository]([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'UserRepository_Administrator'))
                    BEGIN
                        CREATE TABLE [dbo].[UserRepository_Administrator] (
                            [User_Username] VarChar(255) Not Null,
                            [Repository_Name] VarChar(255) Not Null,
                            Constraint [UNQ_UserRepository_Administrator_1] Unique ([User_Username], [Repository_Name]),
                            Foreign Key ([User_Username]) References [User]([Username]),
                            Foreign Key ([Repository_Name]) References [Repository]([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'UserRepository_Permission'))
                    BEGIN
                        CREATE TABLE [dbo].[UserRepository_Permission] (
                            [User_Username] VarChar(255) Not Null,
                            [Repository_Name] VarChar(255) Not Null,
                            Constraint [UNQ_UserRepository_Permission_1] Unique ([User_Username], [Repository_Name]),
                            Foreign Key ([User_Username]) References [User]([Username]),
                            Foreign Key ([Repository_Name]) References [Repository]([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'UserRole_InRole'))
                    BEGIN
                        CREATE TABLE [dbo].[UserRole_InRole] (
                            [User_Username] VarChar(255) Not Null,
                            [Role_Name] VarChar(255) Not Null,
                            Constraint [UNQ_UserRole_InRole_1] Unique ([User_Username], [Role_Name]),
                            Foreign Key ([User_Username]) References [User]([Username]),
                            Foreign Key ([Role_Name]) References [Role]([Name])
                        );
                    END

                    IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'UserTeam_Member'))
                    BEGIN
                        CREATE TABLE [dbo].[UserTeam_Member] (
                            [User_Username] VarChar(255) Not Null,
                            [Team_Name] VarChar(255) Not Null,
                            Constraint [UNQ_UserTeam_Member_1] Unique ([User_Username], [Team_Name]),
                            Foreign Key ([User_Username]) References [User]([Username]),
                            Foreign Key ([Team_Name]) References [Team]([Name])
                        );
                    END

                    ", (int)RepositoryPushMode.Global)
            );
        }
    }
}
