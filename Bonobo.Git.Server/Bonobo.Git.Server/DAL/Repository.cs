using System;
using System.Collections.Generic;

namespace Bonobo.Git.Server.DAL
{
    public partial class Repository
    {
        public Repository()
        {
            this.Teams = new List<Team>();
            this.Administrators = new List<User>();
            this.Users = new List<User>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool Anonymous { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
        public virtual ICollection<User> Administrators { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
