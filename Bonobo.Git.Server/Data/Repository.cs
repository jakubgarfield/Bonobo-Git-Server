using System;
using System.Collections.Generic;

namespace Bonobo.Git.Server.Data
{
    public partial class Repository
    {
        private ICollection<Team> _teams;
        private ICollection<User> _administrators;
        private ICollection<User> _users;

        public string Name { get; set; }
        public string Group { get; set; }
        public string Description { get; set; }
        public bool Anonymous { get; set; }

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

        public virtual ICollection<User> Administrators
        {
            get
            {
                return _administrators ?? (_administrators = new List<User>());
            }
            set
            {
                _administrators = value;
            }
        }

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

        public bool AuditPushUser { get; set; }
    }
}
