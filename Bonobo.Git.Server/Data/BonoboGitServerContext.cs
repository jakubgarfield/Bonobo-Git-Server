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

        [InjectionConstructor]
        public BonoboGitServerContext()
            : base("Name=BonoboGitServerContext")
        {
        }

        public BonoboGitServerContext(DbConnection databaseConnection) : base(databaseConnection, false)
        {
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
