using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class RoleMap : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            SetPrimaryKey(builder);
            SetProperties(builder);
            SetTableAndColumnMappings(builder);
            SetRelationships(builder);
        }

        private void SetRelationships(EntityTypeBuilder<Role> builder)
        {
            builder
                .HasMany(t => t.Users)
                .WithOne(ur => ur.Role)
                .HasForeignKey(t => t.RoleId);
        }

        private void SetTableAndColumnMappings(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Role");
            builder.Property(t => t.Id).HasColumnName("Id");
            builder.Property(t => t.Name).HasColumnName("Name");
            builder.Property(t => t.Description).HasColumnName("Description");
        }

        private void SetProperties(EntityTypeBuilder<Role> builder)
        {
            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(t => t.Description)
                .HasMaxLength(255);
        }

        private void SetPrimaryKey(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(t => t.Id);
        }
    }
}
