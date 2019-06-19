using System;

namespace Bonobo.Git.Server.Data.ManyToMany
{
    public class TeamRepository_Permission
    {
        public Guid RepositoryId { get; set; }
        public Guid TeamId { get; set; }

        public virtual Repository Repository { get; set; }
        public virtual Team Team { get; set; }
    }
}
