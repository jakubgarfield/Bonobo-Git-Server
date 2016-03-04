using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class RepositoryMap : EntityTypeConfiguration<Repository>
    {
        public RepositoryMap()
        {
            SetPrimaryKey();
            SetProperties();
            SetTableAndColumnMappings();
            SetRelationships();
        }

        private void SetRelationships()
        {
            HasMany(t => t.Teams)
                .WithMany(t => t.Repositories)
                .Map(m =>
                {
                    m.ToTable("TeamRepository_Permission");
                    m.MapLeftKey("Repository_Id");
                    m.MapRightKey("Team_Id");
                });

            HasMany(t => t.Administrators)
                .WithMany(t => t.AdministratedRepositories)
                .Map(m =>
                {
                    m.ToTable("UserRepository_Administrator");
                    m.MapLeftKey("Repository_Id");
                    m.MapRightKey("User_Id");
                });

            HasMany(t => t.Users)
                .WithMany(t => t.Repositories)
                .Map(m =>
                {
                    m.ToTable("UserRepository_Permission");
                    m.MapLeftKey("Repository_Id");
                    m.MapRightKey("User_Id");
                });
        }

        private void SetTableAndColumnMappings()
        {
            ToTable("Repository");
            Property(t => t.Id).HasColumnName("Id");
            Property(t => t.Name).HasColumnName("Name");
            Property(t => t.Group).HasColumnName("Group");
            Property(t => t.Description).HasColumnName("Description");
            Property(t => t.Anonymous).HasColumnName("Anonymous");
            Property(t => t.AuditPushUser).HasColumnName("AuditPushUser");
            Property(t => t.AllowAnonymousPush).HasColumnName("AllowAnonymousPush");
        }

        private void SetProperties()
        {
            Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.Group)
                .HasMaxLength(255);

            Property(t => t.Description)
                .HasMaxLength(255);
        }

        private void SetPrimaryKey()
        {
            HasKey(t => t.Id);
        }
    }
}
