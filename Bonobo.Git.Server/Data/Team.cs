using System;
using System.Collections.Generic;
using Bonobo.Git.Server.Data.Mapping;

namespace Bonobo.Git.Server.Data
{
    public partial class Team
    {
        private ICollection<TeamRepositoryPermission> _repositories;
        private ICollection<UserTeamMember> _userTeamMember;


        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<TeamRepositoryPermission> Repositories
        {
            get
            {
                return _repositories ?? (_repositories = new List<TeamRepositoryPermission>());
            }
            set
            {
                _repositories = value;
            }
        }

        public virtual ICollection<UserTeamMember> Users
        {
            get
            {
                return _userTeamMember ?? (_userTeamMember = new List<UserTeamMember>());
            }
            set
            {
                _userTeamMember = value;
            }
        }
    }
}
