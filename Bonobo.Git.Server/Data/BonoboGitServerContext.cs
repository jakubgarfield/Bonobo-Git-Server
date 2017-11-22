using Bonobo.Git.Server.Data.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Bonobo.Git.Server.Data
{
    public partial class BonoboGitServerContext : DbContext
    {
        public DbSet<Repository> Repositories { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<User> Users { get; set; }


        //static BonoboGitServerContext()
        //{
        //    Database.SetInitializer<BonoboGitServerContext>(null);
        //}

        public BonoboGitServerContext(DbContextOptions databaseConnection) : base(databaseConnection)
        {
        }

        //public static BonoboGitServerContext FromDatabase(DbConnection databaseConnection)
        //{
        //    return new BonoboGitServerContext(databaseConnection);
        //}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserTeamMemberMap());
            modelBuilder.ApplyConfiguration(new TeamRepositoryPermissionMap());
            modelBuilder.ApplyConfiguration(new UserRepositoryAdministratorMap());
            modelBuilder.ApplyConfiguration(new UserRepositoryPermissioneMap());
            modelBuilder.ApplyConfiguration(new RepositoryMap());
            modelBuilder.ApplyConfiguration(new RoleMap());
            modelBuilder.ApplyConfiguration(new TeamMap());
            modelBuilder.ApplyConfiguration(new UserMap());
            modelBuilder.ApplyConfiguration(new UserRoleMap());
        }
    }
}
