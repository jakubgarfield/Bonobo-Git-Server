using System;
using System.Collections.Generic;

namespace Bonobo.Git.Server.Data
{
    public partial class Role
    {
        private ICollection<User> _users;

        
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<User> Users
        {
            get
            {
                return _users ?? (_users = new List<User>());
            }
            set
            {
                _users = value;
            }
        }
    }
}
