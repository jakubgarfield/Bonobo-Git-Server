using System;
using System.Collections.Generic;
using Bonobo.Git.Server.Data.ManyToMany;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Data
{
    public partial class User
    {
        private ICollection<UserRepository_Administrator> _administratedRepositories;
        private ICollection<UserRepository_Permission> _repositories;
        private ICollection<UserRole_InRole> _roles;
        private ICollection<UserTeam_Member> _teams;

        public Guid Id { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string Email { get; set; }

        public virtual ICollection<UserRepository_Administrator> AdministratedRepositories
        {
            get
            {
                return _administratedRepositories ?? (_administratedRepositories = new List<UserRepository_Administrator>());
            }
            set
            {
                _administratedRepositories = value;
            }
        }

        public virtual ICollection<UserRepository_Permission> Repositories
        {
            get
            {
                return _repositories ?? (_repositories = new List<UserRepository_Permission>());
            }
            set
            {
                _repositories = value;
            }
        }

        public virtual ICollection<UserRole_InRole> Roles
        {
            get
            {
                return _roles ?? (_roles = new List<UserRole_InRole>());
            }
            set
            {
                _roles = value;
            }
        }

        public virtual ICollection<UserTeam_Member> Teams
        {
            get
            {
                return _teams ?? (_teams = new List<UserTeam_Member>());
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
