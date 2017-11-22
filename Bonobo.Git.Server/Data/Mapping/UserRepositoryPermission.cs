using System;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class UserRepositoryPermission
    {
        public Guid RepositoryId { get; set; }
        public Repository Repository { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
