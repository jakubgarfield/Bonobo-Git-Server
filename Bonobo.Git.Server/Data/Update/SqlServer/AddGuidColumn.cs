﻿using Bonobo.Git.Server.Security;
using System.DirectoryServices.AccountManagement;
using System.Web.Mvc;
using System.Collections.Generic;
using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Bonobo.Git.Server.Helpers;

namespace Bonobo.Git.Server.Data.Update.SqlServer
{
    public class AddGuidColumn : IUpdateScript

    {
        private readonly IAuthenticationProvider AuthProvider;
        private Database _db;

        public AddGuidColumn()
        {
            AuthProvider = DependencyResolver.Current.GetService<IAuthenticationProvider>();
        }

        public void CodeAction(BonoboGitServerContext context)
        {
            _db = context.Database;

            if (UpgradeHasAlreadyBeenRun())
            {
                return;
            }

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    RenameTables();
                    CreateTables();
                    CopyData();
                    AddRelations();
                    DropRenamedTables();
                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        private bool UpgradeHasAlreadyBeenRun()
        {
            try
            {
                var result = _db.SqlQuery<int>(@"
                            IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'User' AND  COLUMN_NAME = 'Id')
                                SELECT 1
                            ELSE
                                SELECT 0
                ");
                return result.SingleAsync().GetAwaiter().GetResult() == 1;
            }
            catch (SqlException)
            {
                // the column does not exist!
                return false;
            }
        }

        void RenameTables()
        {
            _db.ExecuteSqlCommand(@"
                EXEC sp_rename 'User', 'oUser';
                EXEC sp_rename 'Repository', 'oRepo';
                EXEC sp_rename 'Team', 'oTeam';
                EXEC sp_rename 'Role', 'oRole';
                EXEC sp_rename 'UserRepository_Administrator', 'ura';
                EXEC sp_rename 'UserRepository_Permission', 'urp';
                EXEC sp_rename 'UserTeam_Member', 'utm';
                EXEC sp_rename 'UserRole_InRole', 'urir';
                EXEC sp_rename 'TeamRepository_Permission', 'trp';
            ");
        }

        void DropRenamedTables()
        {
            _db.ExecuteSqlCommand(@"
                DROP TABLE ura;
                DROP TABLE urp;
                DROP TABLE utm;
                DROP TABLE urir;
                DROP TABLE trp;
                DROP TABLE oUser;
                DROP TABLE oRepo;
                DROP TABLE oTeam;
                DROP TABLE oRole;
            ");
        }

        void CreateTables()
        {
            _db.ExecuteSqlCommand(@"
                CREATE TABLE [User] (
                    Id       UNIQUEIDENTIFIER      PRIMARY KEY
                                           NOT NULL,
                    Name     VARCHAR (255) NOT NULL,
                    Surname  VARCHAR (255) NOT NULL,
                    Username VARCHAR (255) NOT NULL
                                           UNIQUE,
                    Password VARCHAR (255) NOT NULL,
                    PasswordSalt VARCHAR (255) NOT NULL,
                    Email    VARCHAR (255) NOT NULL
                );

                CREATE TABLE Team (
                    Id       UNIQUEIDENTIFIER      PRIMARY KEY
                                           NOT NULL,
                    Name        VARCHAR (255) NOT NULL
                                              UNIQUE,
                    Description VARCHAR (255)
                );

                CREATE TABLE Repository (
                    Id       UNIQUEIDENTIFIER      PRIMARY KEY
                                           NOT NULL,
                    Name          VARCHAR (255) NOT NULL
                                                UNIQUE,
                    Description   VARCHAR (255),
                    Anonymous     BIT           NOT NULL,
                    AuditPushUser BIT           NOT NULL
                                                DEFAULT ('0'),
                    [Group]       VARCHAR (255) DEFAULT (NULL),
                    Logo          [varbinary](max) DEFAULT (NULL) 
                );

                CREATE TABLE [Role] (
                    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                    [Name] VarChar(255) Not Null UNIQUE,
                    [Description] VarChar(255) Null
                );

                CREATE TABLE UserRepository_Administrator (
                    User_Id       UNIQUEIDENTIFIER NOT NULL,
                    Repository_Id UNIQUEIDENTIFIER NOT NULL,
                    CONSTRAINT UNQ_UserRepository_Administrator_12 UNIQUE (
                        User_Id,
                        Repository_Id
                    ),
                    FOREIGN KEY (
                        User_Id
                    )
                    REFERENCES [User] (Id),
                    FOREIGN KEY (
                        Repository_Id
                    )
                    REFERENCES Repository (Id) 
                );

                CREATE TABLE UserRepository_Permission (
                    User_Id       UNIQUEIDENTIFIER NOT NULL,
                    Repository_Id UNIQUEIDENTIFIER NOT NULL,
                    CONSTRAINT UNQ_UserRepository_Permission_12 UNIQUE (
                        User_Id,
                        Repository_Id
                    ),
                    FOREIGN KEY (
                        User_Id
                    )
                    REFERENCES [User] (Id),
                    FOREIGN KEY (
                        Repository_Id
                    )
                    REFERENCES Repository (Id) 
                );

                CREATE TABLE UserTeam_Member (
                    User_Id UNIQUEIDENTIFIER NOT NULL,
                    Team_Id UNIQUEIDENTIFIER NOT NULL,
                    CONSTRAINT UNQ_UserTeam_Member_12 UNIQUE (
                        User_Id,
                        Team_Id
                    ),
                    FOREIGN KEY (
                        User_Id
                    )
                    REFERENCES [User] (Id),
                    FOREIGN KEY (
                        Team_Id
                    )
                    REFERENCES Team (Id) 
                );

                CREATE TABLE UserRole_InRole (
                    User_Id   UNIQUEIDENTIFIER NOT NULL,
                    Role_Id   UNIQUEIDENTIFIER NOT NULL,
                    CONSTRAINT UNQ_UserRole_InRole_12 UNIQUE (
                        User_Id,
                        Role_Id
                    ),
                    FOREIGN KEY (
                        User_Id
                    )
                    REFERENCES [User] (Id),
                    FOREIGN KEY (
                        Role_Id
                    )
                    REFERENCES Role (Id) 
                );

                CREATE TABLE TeamRepository_Permission (
                    Team_Id       UNIQUEIDENTIFIER      NOT NULL,
                    Repository_Id UNIQUEIDENTIFIER NOT NULL,
                    CONSTRAINT UNQ_TeamRepository_Permission_12 UNIQUE (
                        Team_Id,
                        Repository_Id
                    ),
                    FOREIGN KEY (
                        Team_Id
                    )
                    REFERENCES Team (Id),
                    FOREIGN KEY (
                        Repository_Id
                    )
                    REFERENCES Repository (Id) 
                );

            ");
        }

        class OldUser
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
        }

        class NameDesc
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }

        class OldRepo
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public bool Anonymous { get; set; }
            public bool AuditPushUser { get; set; }
            public string Group { get; set; }
            public byte[] Logo { get; set; }
        }

        private void CopyData()
        {
            CopyUsers();
            CopyTeams();
            CopyRoles();
            CopyRepositories();
        }

        private void CopyUsers()
        {
            var users = _db.SqlQuery<OldUser>("Select * from oUser;").ToList();
            foreach (var entry in users)
            {
                Guid guid = Guid.NewGuid();
                if (AuthProvider is WindowsAuthenticationProvider)
                {
                    var user = ADHelper.GetUserPrincipal(entry.Username);
                    // if the user no longer exists
                    // it means he cannot login anymore so it is safe to assign
                    // any new guid to him
                    if (user != null)
                    {
                        guid = user.Guid.Value;
                        if (string.IsNullOrEmpty(entry.Email) || entry.Email.Equals("None", StringComparison.OrdinalIgnoreCase))
                        {
                            entry.Email = user.EmailAddress;
                        }
                        if (string.IsNullOrEmpty(entry.Surname) ||
                            entry.Surname.Equals("None", StringComparison.OrdinalIgnoreCase))
                        {
                            entry.Surname = user.Surname;
                        }
                        if (string.IsNullOrEmpty(entry.Name) ||
                            entry.Name.Equals(entry.Username, StringComparison.OrdinalIgnoreCase))
                        {
                            entry.Name = user.GivenName;
                        }
                    }
                }
                else
                {
                    // just make sure the admin user has the same guid everywhere. This should make it
                    // easier to identify this user
                    if (entry.Name == "admin")
                    {
                        guid = new Guid("3eb9995e-99e3-425a-b978-1409bdd61fb6");
                    }
                }
                // Existing users will have had passwords which were salted with their username, so we need to replicate that into the Salt column
                var salt = entry.Username;
                _db.ExecuteSqlCommand("INSERT INTO [User] VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                    guid.ToString(), entry.Name, entry.Surname, entry.Username, entry.Password, salt, entry.Email);
            }
        }

        private void CopyTeams()
        {
            var teams = _db.SqlQuery<NameDesc>("Select * from oTeam").ToList();
            foreach (var team in teams)
            {
                _db.ExecuteSqlCommand("INSERT INTO Team VALUES ({0}, {1}, {2})",
                    Guid.NewGuid(), team.Name, team.Description);
            }
        }

        private void CopyRoles()
        {
            var roles = _db.SqlQuery<NameDesc>("Select * from oRole").ToList();
            foreach (var role in roles)
            {
                _db.ExecuteSqlCommand("INSERT INTO Role VALUES ({0}, {1}, {2})",
                    // Administrator is a default role and should have the same Guid on all systems to make debugging easier
                    role.Name.Equals("Administrator") ? new Guid("a3139d2b-5a59-427f-bb2d-af251dce00e4") : Guid.NewGuid(),
                    role.Name, role.Description);
            }
        }

        private void CopyRepositories()
        {
            var repos = _db.SqlQuery<OldRepo>("SELECT * FROM oRepo").ToList();
            foreach (var repo in repos)
            {
                _db.ExecuteSqlCommand("INSERT INTO Repository VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                    Guid.NewGuid(), repo.Name, repo.Description, repo.Anonymous, repo.AuditPushUser, repo.Group, repo.Logo);
            }
        }
        
        private void AddRelations()
        {
            // ALTER TABLE UserRepository_Administrator RENAME TO ura;
            _db.ExecuteSqlCommand(@"
                INSERT INTO UserRepository_Administrator (
                                                             User_Id,
                                                             Repository_Id
                                                         )
                                                         SELECT [User].Id,
                                                                Repository.Id
                                                           FROM ura JOIN [User]
                                                        ON ura.User_Username = [User].Username
                                                                JOIN Repository
                                                          ON ura.Repository_Name = Repository.Name;
            INSERT INTO UserRepository_Permission (
                                                          User_Id,
                                                          Repository_Id
                                                      )
                                                      SELECT [User].Id,
                                                             Repository.Id
                                                        FROM urp JOIN [User] 
                                                            ON urp.User_Username = [User].Username
                                                             Join Repository
                                                       ON urp.Repository_Name = Repository.Name;

                INSERT INTO UserTeam_Member (
                                                User_Id,
                                                Team_Id
                                            )
                                            SELECT [User].Id,
                                                   Team.Id
                                              FROM utm
                                                   JOIN
                                                   [User] ON utm.User_Username = [User].Username
                                                   JOIN
                                                   Team
                                             ON
                                                   utm.Team_Name = Team.Name;

                INSERT INTO UserRole_InRole (
                                                User_Id,
                                                Role_Id
                                            )
                                            SELECT [User].Id,
                                                   Role.Id
                                              FROM urir
                                                   JOIN
                                                   [User] ON urir.User_Username = [User].Username
                                                   JOIN
                                                   Role
                                             ON urir.Role_Name = Role.Name;

                INSERT INTO TeamRepository_Permission (
                                                          Team_Id,
                                                          Repository_Id
                                                      )
                                                      SELECT Team.Id,
                                                             Repository.Id
                                                        FROM trp
                                                             JOIN
                                                             Team ON trp.Team_Name = Team.Name
                                                             JOIN
                                                             Repository
                                                        ON trp.Repository_Name = Repository.Name;

            ");
        }

        public string Command { get { return null; } }
        public string Precondition { get { return null; } }

        public PrincipalContext ADPricipalContextHelper { get; private set; }
    }
}
