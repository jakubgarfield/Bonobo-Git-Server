using System;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class TeamRepositoryPermission
    {
        public Guid RepositoryId { get; set; }
        public Repository Repository { get; set; }
        public Guid TeamId { get; set; }
        public Team Team { get; set; }
    }
}
