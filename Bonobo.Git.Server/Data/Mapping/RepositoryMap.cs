using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class RepositoryMap : IEntityTypeConfiguration<Repository>
    {

        public void Configure(EntityTypeBuilder<Repository> builder)
        {
            SetPrimaryKey(builder);
            SetProperties(builder);
            SetTableAndColumnMappings(builder);
            SetRelationships(builder);
        }

        private void SetRelationships(EntityTypeBuilder<Repository> builder)
        {
            builder.HasMany(t => t.Teams)
                .WithOne(r => r.Repository)
                .HasForeignKey(x => x.TeamId);

            builder.HasMany(t => t.Administrators)
                .WithOne(t => t.Repository)
                .HasForeignKey(x => x.UserId);

            builder.HasMany(t => t.Users)
                .WithOne(t => t.Repository)
                .HasForeignKey(x => x.UserId);
        }

        private void SetTableAndColumnMappings(EntityTypeBuilder<Repository> builder)
        {
            builder.ToTable("Repository");
            builder.Property(t => t.Id).HasColumnName("Id");
            builder.Property(t => t.Name).HasColumnName("Name");
            builder.Property(t => t.Group).HasColumnName("Group");
            builder.Property(t => t.Description).HasColumnName("Description");
            builder.Property(t => t.Anonymous).HasColumnName("Anonymous");
            builder.Property(t => t.AuditPushUser).HasColumnName("AuditPushUser");
            builder.Property(t => t.AllowAnonymousPush).HasColumnName("AllowAnonymousPush");
            builder.Property(t => t.LinksRegex).HasColumnName("LinksRegex");
            builder.Property(t => t.LinksUrl).HasColumnName("LinksUrl");
            builder.Property(t => t.LinksUseGlobal).HasColumnName("LinksUseGlobal");
        }

        private void SetProperties(EntityTypeBuilder<Repository> builder)
        {
            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(t => t.Group)
                .HasMaxLength(255);

            builder.Property(t => t.Description)
                .HasMaxLength(255);
        }

        private void SetPrimaryKey(EntityTypeBuilder<Repository> builder)
        {
            builder.HasKey(t => t.Id);
        }
    }
}
