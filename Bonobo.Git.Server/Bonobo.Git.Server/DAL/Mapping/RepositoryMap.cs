using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Bonobo.Git.Server.DAL.Mapping
{
    public class RepositoryMap : EntityTypeConfiguration<Repository>
    {
        public RepositoryMap()
        {
            // Primary Key
            this.HasKey(t => t.Name);

            // Properties
            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.Description)
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("Repository");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.Anonymous).HasColumnName("Anonymous");

            // Relationships
            this.HasMany(t => t.Teams)
                .WithMany(t => t.Repositories)
                .Map(m =>
                    {
                        m.ToTable("TeamRepository_Permission");
                        m.MapLeftKey("Repository_Name");
                        m.MapRightKey("Team_Name");
                    });

            this.HasMany(t => t.Administrators)
                .WithMany(t => t.AdministratedRepositories)
                .Map(m =>
                    {
                        m.ToTable("UserRepository_Administrator");
                        m.MapLeftKey("Repository_Name");
                        m.MapRightKey("User_Username");
                    });

            this.HasMany(t => t.Users)
                .WithMany(t => t.Repositories)
                .Map(m =>
                    {
                        m.ToTable("UserRepository_Permission");
                        m.MapLeftKey("Repository_Name");
                        m.MapRightKey("User_Username");
                    });


        }
    }
}
