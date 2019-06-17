using System;

namespace Bonobo.Git.Server.Data.ManyToMany
{
    public class UserRepository_Permission
    {
        public Guid RepositoryId { get; set; }
        public Guid UserId { get; set; }

        public virtual Repository Repository { get; set; }
        public virtual User User { get; set; }
    }
}
