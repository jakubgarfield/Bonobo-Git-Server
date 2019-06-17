using System;

namespace Bonobo.Git.Server.Data.ManyToMany
{
    public class UserRole_InRole
    {
        public Guid RoleId { get; set; }
        public Guid UserId { get; set; }

        public virtual Role Role { get; set; }
        public virtual User User { get; set; }
    }
}
