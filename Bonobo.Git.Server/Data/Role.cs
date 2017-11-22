using System;
using System.Collections.Generic;

namespace Bonobo.Git.Server.Data
{
    public class Role
    {
        private ICollection<UserRole> _userRoles;

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<UserRole> Users
        {
            get
            {
                return _userRoles ?? (_userRoles = new List<UserRole>());
            }
            set
            {
                _userRoles = value;
            }
        }
    }
}
