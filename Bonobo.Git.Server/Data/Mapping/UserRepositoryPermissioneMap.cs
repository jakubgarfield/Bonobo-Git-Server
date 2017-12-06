using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class UserRepositoryPermissioneMap : IEntityTypeConfiguration<UserRepositoryPermission>
    {
        public void Configure(EntityTypeBuilder<UserRepositoryPermission> builder)
        {
            builder.ToTable("UserRepository_Permission");
            builder.HasKey(ur => new { ur.RepositoryId, ur.UserId });

            builder.Property(t => t.UserId).HasColumnName("User_Id").IsRequired();
            builder.Property(t => t.RepositoryId).HasColumnName("Repository_Id").IsRequired();

            builder
                .HasOne(bc => bc.User)
                .WithMany(b => b.Repositories)
                .HasForeignKey(bc => bc.UserId);

            builder
                .HasOne(bc => bc.Repository)
                .WithMany(c => c.Users)
                .HasForeignKey(bc => bc.RepositoryId);
        }
    }
}
