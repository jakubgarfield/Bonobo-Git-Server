using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Bonobo.Git.Server.DAL.Mapping;

namespace Bonobo.Git.Server.DAL
{
    public partial class BonoboGitServerContext : DbContext
    {
        static BonoboGitServerContext()
        {
            Database.SetInitializer<BonoboGitServerContext>(null);
        }

        public BonoboGitServerContext()
            : base("Name=BonoboGitServerContext")
        {
        }

        public DbSet<Repository> Repositories { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new RepositoryMap());
            modelBuilder.Configurations.Add(new RoleMap());
            modelBuilder.Configurations.Add(new TeamMap());
            modelBuilder.Configurations.Add(new UserMap());
        }
    }
}
