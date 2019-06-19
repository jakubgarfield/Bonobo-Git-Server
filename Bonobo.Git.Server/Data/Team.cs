using System;
using System.Collections.Generic;
using Bonobo.Git.Server.Data.ManyToMany;

namespace Bonobo.Git.Server.Data
{
    public partial class Team
    {
        private ICollection<TeamRepository_Permission> _repositories;
        private ICollection<UserTeam_Member> _users;


        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<TeamRepository_Permission> Repositories
        {
            get
            {
                return _repositories ?? (_repositories = new List<TeamRepository_Permission>());
            }
            set
            {
                _repositories = value;
            }
        }

        public virtual ICollection<UserTeam_Member> Users
        {
            get
            {
                return _users ?? (_users = new List<UserTeam_Member>());
            }
            set
            {
                _users = value;
            }
        }
    }
}
