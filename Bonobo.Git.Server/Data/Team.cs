using System;
using System.Collections.Generic;

namespace Bonobo.Git.Server.Data
{
    public partial class Team
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<Repository> Repositories { get; set; }
        public virtual ICollection<User> Users { get; set; }
    

        public Team()
        {
        }
    }
}
