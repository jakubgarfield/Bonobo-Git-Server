using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class RoleMap : IEntityTypeConfiguration<Role>
    {
        private readonly ValueConverter _primaryKeyConverter;

        public RoleMap(ValueConverter primaryKeyConverter)
        {
            _primaryKeyConverter = primaryKeyConverter;
        }

        public void Configure(EntityTypeBuilder<Role> builder)
        {
            SetPrimaryKey(builder);
            SetProperties(builder);
            SetTableAndColumnMappings(builder);
            SetRelationships(builder);
        }

        private void SetRelationships(EntityTypeBuilder<Role> builder)
        {
            //builder.HasMany(t => t.Users)
            //    .WithMany(t => t.Roles)
            //    .Map(m =>
            //    {
            //        m.ToTable("UserRole_InRole");
            //        m.MapLeftKey("Role_Id");
            //        m.MapRightKey("User_Id");
            //    });
        }

        private void SetTableAndColumnMappings(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Role");
            builder.Property(t => t.Id).HasColumnName("Id").HasConversion(_primaryKeyConverter);
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
