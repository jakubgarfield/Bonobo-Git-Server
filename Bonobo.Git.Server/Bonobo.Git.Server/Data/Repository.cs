using System;
using System.Collections.Generic;

namespace Bonobo.Git.Server.Data
{
    public partial class Repository
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Anonymous { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
        public virtual ICollection<User> Administrators { get; set; }
        public virtual ICollection<User> Users { get; set; }


        public Repository()
        {
            Teams = new List<Team>();
            Administrators = new List<User>();
            Users = new List<User>();
        }
    }
}
