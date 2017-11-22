using System;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class UserTeamMember
    {
        public Guid TeamId { get; set; }
        public Team Team { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
