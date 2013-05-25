using System;
using System.Collections.Generic;

namespace Bonobo.Git.Server.Data
{
    public partial class Role
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<User> Users { get; set; }


        public Role()
        {
            Users = new List<User>();
        }
    }
}
