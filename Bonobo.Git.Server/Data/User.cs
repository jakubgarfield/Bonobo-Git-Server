using System;
using System.Collections.Generic;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Data
{
    public partial class User
    {
        private ICollection<Repository> _administratedRepositories;
        private ICollection<Repository> _repositories;
        private ICollection<Role> _roles;
        private ICollection<Team> _teams;

        public Guid Id { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string Email { get; set; }

        public virtual ICollection<Repository> AdministratedRepositories
        {
            get
            {
                return _administratedRepositories ?? (_administratedRepositories = new List<Repository>());
            }
            set
            {
                _administratedRepositories = value;
            }
        }

        public virtual ICollection<Repository> Repositories
        {
            get
            {
                return _repositories ?? (_repositories = new List<Repository>());
            }
            set
            {
                _repositories = value;
            }
        }

        public virtual ICollection<Role> Roles
        {
            get
            {
                return _roles ?? (_roles = new List<Role>());
            }
            set
            {
                _roles = value;
            }
        }

        public virtual ICollection<Team> Teams
        {
            get
            {
                return _teams ?? (_teams = new List<Team>());
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
