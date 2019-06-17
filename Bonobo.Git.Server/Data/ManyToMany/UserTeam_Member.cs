using System;

namespace Bonobo.Git.Server.Data.ManyToMany
{
    public class UserTeam_Member
    {
        public Guid TeamId { get; set; }
        public Guid UserId { get; set; }

        public virtual Team Team { get; set; }
        public virtual User User { get; set; }
    }
}
