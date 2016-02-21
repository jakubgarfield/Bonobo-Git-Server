using System.Data.Common;
using Bonobo.Git.Server.Data.Mapping;
using System.Data.Entity;
using Microsoft.Practices.Unity;

namespace Bonobo.Git.Server.Data
{
    public partial class BonoboGitServerContext : DbContext
    {
        public DbSet<Repository> Repositories { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<User> Users { get; set; }


        static BonoboGitServerContext()
        {
            Database.SetInitializer<BonoboGitServerContext>(null);
        }

        public BonoboGitServerContext()
            : base("Name=BonoboGitServerContext")
        {
        }

        // Don't make this public because it confuses Unity
        private BonoboGitServerContext(DbConnection databaseConnection) : base(databaseConnection, false)
        {
        }

        public static BonoboGitServerContext FromDatabase(DbConnection databaseConnection)
        {
            return new BonoboGitServerContext(databaseConnection);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new RepositoryMap());
            modelBuilder.Configurations.Add(new RoleMap());
            modelBuilder.Configurations.Add(new TeamMap());
            modelBuilder.Configurations.Add(new UserMap());
        }
    }
}
