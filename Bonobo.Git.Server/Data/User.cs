using System;
using System.Collections.Generic;
using Bonobo.Git.Server.Data.Mapping;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Data
{
    public partial class User
    {
        private ICollection<UserRepositoryPermission> _userRepositoryPermissions;
        private ICollection<UserRepositoryAdministrator> _administratedRepositories;
        //private ICollection<TeamRepositoryPermission> _repositories;
        private ICollection<UserRole> _roles;
        private ICollection<UserTeamMember> _teams;

        public Guid Id { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string Email { get; set; }

        public virtual ICollection<UserRepositoryPermission> Repositories
        {
            get
            {
                return _userRepositoryPermissions ?? (_userRepositoryPermissions = new List<UserRepositoryPermission>());
            }
            set
            {
                _userRepositoryPermissions = value;
            }
        }

        public virtual ICollection<UserRepositoryAdministrator> AdministratedRepositories
        {
            get
            {
                return _administratedRepositories ?? (_administratedRepositories = new List<UserRepositoryAdministrator>());
            }
            set
            {
                _administratedRepositories = value;
            }
        }

        //public virtual ICollection<TeamRepositoryPermission> Repositories
        //{
        //    get
        //    {
        //        return _repositories ?? (_repositories = new List<TeamRepositoryPermission>());
        //    }
        //    set
        //    {
        //        _repositories = value;
        //    }
        //}

        public virtual ICollection<UserRole> Roles
        {
            get
            {
                return _roles ?? (_roles = new List<UserRole>());
            }
            set
            {
                _roles = value;
            }
        }

        public virtual ICollection<UserTeamMember> Teams
        {
            get
            {
                return _teams ?? (_teams = new List<UserTeamMember>());
            }
            set
            {
                _teams = value;
            }
        }

        public UserModel ToModel()
        {
            return new UserModel
            {
                Id = Id,
                Username = Username,
                GivenName = GivenName,
                Surname = Surname,
                Email = Email,
            };
        }
    }
}
