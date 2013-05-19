using System;
using System.Collections.Generic;

namespace Bonobo.Git.Server.DAL
{
    public partial class User
    {
        public User()
        {
            this.AdministratedRepositories = new List<Repository>();
            this.Repositories = new List<Repository>();
            this.Roles = new List<Role>();
            this.Teams = new List<Team>();
        }

        public string Name { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public virtual ICollection<Repository> AdministratedRepositories { get; set; }
        public virtual ICollection<Repository> Repositories { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
    }
}
