using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class RoleMap : EntityTypeConfiguration<Role>
    {
        public RoleMap()
        {
            SetPrimaryKey();
            SetProperties();
            SetTableAndColumnMappings();
            SetRelationships();
        }


        private void SetRelationships()
        {
            HasMany(t => t.Users)
                .WithMany(t => t.Roles)
                .Map(m =>
                {
                    m.ToTable("UserRole_InRole");
                    m.MapLeftKey("Role_Name");
                    m.MapRightKey("User_Username");
                });
        }

        private void SetTableAndColumnMappings()
        {
            ToTable("Role");
            Property(t => t.Name).HasColumnName("Name");
            Property(t => t.Description).HasColumnName("Description");
        }

        private void SetProperties()
        {
            Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.Description)
                .HasMaxLength(255);
        }

        private void SetPrimaryKey()
        {
            HasKey(t => t.Name);
        }
    }
}
