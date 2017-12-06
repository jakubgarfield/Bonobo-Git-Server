using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class UserRoleMap : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRole_InRole");
            builder.HasKey(ur => new { ur.UserId, ur.RoleId });

            builder.Property(t => t.UserId).HasColumnName("User_Id").IsRequired();
            builder.Property(t => t.RoleId).HasColumnName("Role_Id").IsRequired();

            builder
                .HasOne(bc => bc.User)
                .WithMany(b => b.Roles)
                .HasForeignKey(bc => bc.UserId);

            builder
                .HasOne(bc => bc.Role)
                .WithMany(c => c.Users)
                .HasForeignKey(bc => bc.RoleId);
        }
    }
}
