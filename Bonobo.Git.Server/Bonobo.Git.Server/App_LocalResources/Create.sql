CREATE TABLE [Repository] (
    [Name] VarChar(255) Not Null,
    [Description] VarChar(255) Null,
    [Anonymous] Bit Not Null,
    Constraint [PK_Repository] Primary Key ([Name])
);

CREATE TABLE [Role] (
    [Name] VarChar(255) Not Null,
    [Description] VarChar(255) Null,
    Constraint [PK_Role] Primary Key ([Name])
);

CREATE TABLE [Team] (
    [Name] VarChar(255) Not Null,
    [Description] VarChar(255) Null,
    Constraint [PK_Team] Primary Key ([Name])
);

CREATE TABLE [User] (
    [Name] VarChar(255) Not Null,
    [Surname] VarChar(255) Not Null,
    [Username] VarChar(255) Not Null,
    [Password] VarChar(255) Not Null,
    [Email] VarChar(255) Not Null,
    Constraint [PK_User] Primary Key ([Username])
);

CREATE TABLE [TeamRepository_Permission] (
    [Team_Name] VarChar(255) Not Null,
    [Repository_Name] VarChar(255) Not Null,
    Constraint [UNQ_TeamRepository_Permission_1] Unique ([Team_Name], [Repository_Name]),
    Foreign Key ([Team_Name]) References [Team]([Name]),
    Foreign Key ([Repository_Name]) References [Repository]([Name])
);

CREATE TABLE [UserRepository_Administrator] (
    [User_Username] VarChar(255) Not Null,
    [Repository_Name] VarChar(255) Not Null,
    Constraint [UNQ_UserRepository_Administrator_1] Unique ([User_Username], [Repository_Name]),
    Foreign Key ([User_Username]) References [User]([Username]),
    Foreign Key ([Repository_Name]) References [Repository]([Name])
);

CREATE TABLE [UserRepository_Permission] (
    [User_Username] VarChar(255) Not Null,
    [Repository_Name] VarChar(255) Not Null,
    Constraint [UNQ_UserRepository_Permission_1] Unique ([User_Username], [Repository_Name]),
    Foreign Key ([User_Username]) References [User]([Username]),
    Foreign Key ([Repository_Name]) References [Repository]([Name])
);

CREATE TABLE [UserRole_InRole] (
    [User_Username] VarChar(255) Not Null,
    [Role_Name] VarChar(255) Not Null,
    Constraint [UNQ_UserRole_InRole_1] Unique ([User_Username], [Role_Name]),
    Foreign Key ([User_Username]) References [User]([Username]),
    Foreign Key ([Role_Name]) References [Role]([Name])
);

CREATE TABLE [UserTeam_Member] (
    [User_Username] VarChar(255) Not Null,
    [Team_Name] VarChar(255) Not Null,
    Constraint [UNQ_UserTeam_Member_1] Unique ([User_Username], [Team_Name]),
    Foreign Key ([User_Username]) References [User]([Username]),
    Foreign Key ([Team_Name]) References [Team]([Name])
);

--ID:admin; Pswd:admin
INSERT INTO [User] ([Name], [Surname], [Username], [Password], [Email]) VALUES ('admin', '', 'admin', '21232F297A57A5A743894A0E4A801FC3', '');
INSERT INTO [Role] ([Name], [Description]) VALUES ('Administrator','System administrator');
INSERT INTO [UserRole_InRole] ([User_Username], [Role_Name]) VALUES ('admin', 'Administrator');
