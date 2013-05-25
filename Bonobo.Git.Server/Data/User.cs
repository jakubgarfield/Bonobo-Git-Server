using System;
using System.Collections.Generic;

namespace Bonobo.Git.Server.Data
{
    public partial class User
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public virtual ICollection<Repository> AdministratedRepositories { get; set; }
        public virtual ICollection<Repository> Repositories { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
        public virtual ICollection<Team> Teams { get; set; }

        
        public User()
        {
            AdministratedRepositories = new List<Repository>();
            Repositories = new List<Repository>();
            Roles = new List<Role>();
            Teams = new List<Team>();
        }
    }
}
