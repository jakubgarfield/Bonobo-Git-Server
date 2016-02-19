using Bonobo.Git.Server.Security;
using System.DirectoryServices.AccountManagement;
using System.Web.Mvc;
using System.Collections.Generic;
using System;
using System.Data.Entity;
using System.Data.SQLite;

namespace Bonobo.Git.Server.Data.Update.Sqlite
{
    public class AddGuidColumn
    {
        readonly IAuthenticationProvider AuthProvider;
        readonly Database _db;

        public AddGuidColumn(BonoboGitServerContext context)
        {
            AuthProvider = DependencyResolver.Current.GetService<IAuthenticationProvider>();

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
                // force evaluation to get an error if column does not exist
                _db.ExecuteSqlCommand("SELECT Count([Id]) = -1 FROM User");
                return true;
            }
            catch (SQLiteException)
            {
                // the column does not exist!
                return false;
            }
        }

        void RenameTables()
        {
            _db.ExecuteSqlCommand(@"
                ALTER TABLE User RENAME TO oUser;
                ALTER TABLE Repository RENAME TO oRepo;
                ALTER TABLE Team RENAME TO oTeam;
                ALTER TABLE Role RENAME TO oRole;
                ALTER TABLE UserRepository_Administrator RENAME TO ura;
                ALTER TABLE UserRepository_Permission RENAME TO urp;
                ALTER TABLE UserTeam_Member RENAME TO utm;
                ALTER TABLE UserRole_InRole RENAME TO urir;
                ALTER TABLE TeamRepository_Permission RENAME TO trp;
            ");
        }

        void DropRenamedTables()
        {
            _db.ExecuteSqlCommand(@"
                DROP TABLE oUser;
                DROP TABLE oRepo;
                DROP TABLE oTeam;
                DROP TABLE oRole;
                DROP TABLE ura;
                DROP TABLE urp;
                DROP TABLE utm;
                DROP TABLE urir;
                DROP TABLE trp;
            ");
        }

        void CreateTables()
        {
            _db.ExecuteSqlCommand(@"
                CREATE TABLE User (
                    Id       Char(36)      PRIMARY KEY
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
                    Id       Char(36)      PRIMARY KEY
                                           NOT NULL,
                    Name        VARCHAR (255) NOT NULL
                                              UNIQUE,
                    Description VARCHAR (255)
                );

                CREATE TABLE Repository (
                    Id       Char(36)      PRIMARY KEY
                                           NOT NULL,
                    Name          VARCHAR (255) NOT NULL
                                                UNIQUE,
                    Description   VARCHAR (255),
                    Anonymous     BIT           NOT NULL,
                    AuditPushUser BIT           NOT NULL
                                                DEFAULT ('0'),
                    [Group]       VARCHAR (255) DEFAULT (NULL),
                    Logo          BLOB          DEFAULT (NULL) 
                );

                CREATE TABLE [Role] (
                    [Id] Char(36) PRIMARY KEY,
                    [Name] VarChar(255) Not Null UNIQUE,
                    [Description] VarChar(255) Null
                );

                CREATE TABLE UserRepository_Administrator (
                    User_Id       CHAR(36) NOT NULL,
                    Repository_Id CHAR(36) NOT NULL,
                    CONSTRAINT UNQ_UserRepository_Administrator_1 UNIQUE (
                        User_Id,
                        Repository_Id
                    ),
                    FOREIGN KEY (
                        User_Id
                    )
                    REFERENCES User (Id),
                    FOREIGN KEY (
                        Repository_Id
                    )
                    REFERENCES Repository (Id) 
                );

                CREATE TABLE UserRepository_Permission (
                    User_Id       CHAR(36) NOT NULL,
                    Repository_Id CHAR(36) NOT NULL,
                    CONSTRAINT UNQ_UserRepository_Permission_1 UNIQUE (
                        User_Id,
                        Repository_Id
                    ),
                    FOREIGN KEY (
                        User_Id
                    )
                    REFERENCES User (Id),
                    FOREIGN KEY (
                        Repository_Id
                    )
                    REFERENCES Repository (Id) 
                );

                CREATE TABLE UserTeam_Member (
                    User_Id CHAR(36) NOT NULL,
                    Team_Id CHAR(36) NOT NULL,
                    CONSTRAINT UNQ_UserTeam_Member_1 UNIQUE (
                        User_Id,
                        Team_Id
                    ),
                    FOREIGN KEY (
                        User_Id
                    )
                    REFERENCES User (Id),
                    FOREIGN KEY (
                        Team_Id
                    )
                    REFERENCES Team (Id) 
                );

                CREATE TABLE UserRole_InRole (
                    User_Id   CHAR(36) NOT NULL,
                    Role_Id   CHAR(36) NOT NULL,
                    CONSTRAINT UNQ_UserRole_InRole_1 UNIQUE (
                        User_Id,
                        Role_Id
                    ),
                    FOREIGN KEY (
                        User_Id
                    )
                    REFERENCES User (Id),
                    FOREIGN KEY (
                        Role_Id
                    )
                    REFERENCES Role (Id) 
                );

                CREATE TABLE TeamRepository_Permission (
                    Team_Id       CHAR(36)      NOT NULL,
                    Repository_Id CHAR(36) NOT NULL,
                    CONSTRAINT UNQ_TeamRepository_Permission_1 UNIQUE (
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
            var users = _db.SqlQuery<OldUser>("Select * from oUser;");
            Dictionary<string, PrincipalContext> domains = new Dictionary<string, PrincipalContext>();
            foreach (var entry in users)
            {
                Guid guid = Guid.NewGuid();
                if (AuthProvider is WindowsAuthenticationProvider)
                {
                    var domain = entry.Username.GetDomain(); // not sure what to do if domain is not found...
                    PrincipalContext dc;
                    ;
                    if (!domains.TryGetValue(domain, out dc))
                    {
                        dc = new PrincipalContext(ContextType.Domain, domain);
                        domains[domain] = dc;
                    }
                    var user = UserPrincipal.FindByIdentity(dc, entry.Username);
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
                _db.ExecuteSqlCommand("INSERT INTO User VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                    guid.ToString(), entry.Name, entry.Surname, entry.Username, entry.Password, salt, entry.Email);
            }
        }

        private void CopyTeams()
        {
            var teams = _db.SqlQuery<NameDesc>("Select * from oTeam");
            foreach (var team in teams)
            {
                _db.ExecuteSqlCommand("INSERT INTO Team VALUES ({0}, {1}, {2})",
                    Guid.NewGuid(), team.Name, team.Description);
            }
        }

        private void CopyRoles()
        {
            var roles = _db.SqlQuery<NameDesc>("Select * from oRole");
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
            var repos = _db.SqlQuery<OldRepo>("SELECT * FROM oRepo");
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
                                                         SELECT User.Id,
                                                                Repository.Id
                                                           FROM ura
                                                                JOIN
                                                                User
                                                                JOIN
                                                                Repository
                                                          WHERE ura.User_Username = User.Username
                                                          AND ura.Repository_Name = Repository.Name;

                INSERT INTO UserRepository_Permission (
                                                          User_Id,
                                                          Repository_Id
                                                      )
                                                      SELECT User.Id,
                                                             Repository.Id
                                                        FROM urp
                                                             JOIN
                                                             User
                                                             Join
                                                             Repository
                                                       WHERE urp.User_Username = User.Username
                                                       AND urp.Repository_Name = Repository.Name;

                INSERT INTO UserTeam_Member (
                                                User_Id,
                                                Team_Id
                                            )
                                            SELECT User.Id,
                                                   Team.Id
                                              FROM utm
                                                   JOIN
                                                   User
                                                   JOIN
                                                   Team
                                             WHERE utm.User_Username = User.Username AND 
                                                   utm.Team_Name = Team.Name;

                INSERT INTO UserRole_InRole (
                                                User_Id,
                                                Role_Id
                                            )
                                            SELECT User.Id,
                                                   Role.Id
                                              FROM urir
                                                   JOIN
                                                   User
                                                   JOIN
                                                   Role
                                             WHERE urir.User_Username = User.Username
                                             AND urir.Role_Name = Role.Name;

                INSERT INTO TeamRepository_Permission (
                                                          Team_Id,
                                                          Repository_Id
                                                      )
                                                      SELECT Team.Id,
                                                             Repository.Id
                                                        FROM trp
                                                             JOIN
                                                             Team
                                                             JOIN
                                                             Repository
                                                       WHERE trp.Team_Name = Team.Name
                                                       AND trp.Repository_Name = Repository.Name;

            ");
        }
    }
}
