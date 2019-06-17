using Bonobo.Git.Server.Data.ManyToMany;
using Bonobo.Git.Server.Data.Mapping;
using Bonobo.Git.Server.Data.Update.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bonobo.Git.Server.Data
{
    public class BonoboGitServerContext : DbContext
    {
        public DbSet<Repository> Repositories { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<User> Users { get; set; }

        public BonoboGitServerContext(DbContextOptions<BonoboGitServerContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ValueConverter primaryKeyConverter = null;
            if (Database.IsSqlite())
            {
                primaryKeyConverter = new GuidToStringConverter();
            }

            modelBuilder.ApplyConfiguration(new RepositoryMap(primaryKeyConverter));
            modelBuilder.ApplyConfiguration(new RoleMap(primaryKeyConverter));
            modelBuilder.ApplyConfiguration(new TeamMap(primaryKeyConverter));
            modelBuilder.ApplyConfiguration(new UserMap(primaryKeyConverter));

            modelBuilder.Query<NameDesc>();
            modelBuilder.Query<OldRepo>();
            modelBuilder.Query<OldUser>();

            var builder1 = modelBuilder.Entity<TeamRepository_Permission>();
            builder1.Property(x => x.TeamId).HasColumnName("Team_Id").HasConversion(primaryKeyConverter);
            builder1.Property(x => x.RepositoryId).HasColumnName("Repository_Id").HasConversion(primaryKeyConverter);
            builder1.HasKey(x => new { x.RepositoryId, x.TeamId });

            builder1.HasOne(x => x.Repository)
                .WithMany(r => r.Teams)
                .HasForeignKey(x => x.RepositoryId);

            builder1.HasOne(x => x.Team)
                .WithMany(t => t.Repositories)
                .HasForeignKey(x => x.TeamId);


            var builder2 = modelBuilder.Entity<UserRepository_Administrator>();
            builder2.Property(x => x.UserId).HasColumnName("User_Id").HasConversion(primaryKeyConverter);
            builder2.Property(x => x.RepositoryId).HasColumnName("Repository_Id").HasConversion(primaryKeyConverter);
            builder2.HasKey(x => new {x.RepositoryId, x.UserId});

            builder2.HasOne(x => x.User)
                .WithMany(u => u.AdministratedRepositories)
                .HasForeignKey(x => x.UserId);

            builder2.HasOne(x => x.Repository)
                .WithMany(r => r.Administrators)
                .HasForeignKey(x => x.RepositoryId);


            var builder3 = modelBuilder.Entity<UserRepository_Permission>();
            builder3.Property(x => x.UserId).HasColumnName("User_Id").HasConversion(primaryKeyConverter);
            builder3.Property(x => x.RepositoryId).HasColumnName("Repository_Id").HasConversion(primaryKeyConverter); 
            builder3.HasKey(x => new {x.RepositoryId, x.UserId});

            builder3.HasOne(x => x.User)
                .WithMany(u => u.Repositories)
                .HasForeignKey(x => x.UserId);

            builder3.HasOne(x => x.Repository)
                .WithMany(r => r.Users)
                .HasForeignKey(x => x.RepositoryId);

            var builder4 = modelBuilder.Entity<UserRole_InRole>();
            builder4.Property(x => x.UserId).HasColumnName("User_Id").HasConversion(primaryKeyConverter);
            builder4.Property(x => x.RoleId).HasColumnName("Role_Id").HasConversion(primaryKeyConverter);
            builder4.HasKey(x => new {x.RoleId, x.UserId});

            builder4.HasOne(x => x.User)
                .WithMany(u => u.Roles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder4.HasOne(x => x.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);


            var builder5 = modelBuilder.Entity<UserTeam_Member>();
            builder5.Property(x => x.UserId).HasColumnName("User_Id").HasConversion(primaryKeyConverter);
            builder5.Property(x => x.TeamId).HasColumnName("Team_Id").HasConversion(primaryKeyConverter);
            builder5.HasKey(x => new {x.TeamId, x.UserId});

            builder5.HasOne(x => x.User)
                .WithMany(u => u.Teams)
                .HasForeignKey(x => x.UserId);

            builder5.HasOne(x => x.Team)
                .WithMany(t => t.Users)
                .HasForeignKey(x => x.TeamId);
        }
    }
}
