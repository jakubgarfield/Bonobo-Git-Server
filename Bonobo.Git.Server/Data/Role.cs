using System;
using System.Collections.Generic;
using Bonobo.Git.Server.Data.ManyToMany;

namespace Bonobo.Git.Server.Data
{
    public partial class Role
    {
        private ICollection<UserRole_InRole> _users;

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<UserRole_InRole> Users
        {
            get
            {
                return _users ?? (_users = new List<UserRole_InRole>());
            }
            set
            {
                _users = value;
            }
        }
    }
}
