using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class UserRepositoryAdministratorMap : IEntityTypeConfiguration<UserRepositoryAdministrator>
    {
        public void Configure(EntityTypeBuilder<UserRepositoryAdministrator> builder)
        {
            builder.ToTable("UserRepository_Administrator");
            builder.HasKey(ur => new { ur.UserId, ur.RepositoryId });

            builder.Property(t => t.UserId).HasColumnName("User_Id").IsRequired();
            builder.Property(t => t.RepositoryId).HasColumnName("Repository_Id").IsRequired();

            builder
                .HasOne(bc => bc.User)
                .WithMany(b => b.AdministratedRepositories)
                .HasForeignKey(bc => bc.UserId);

            builder
                .HasOne(bc => bc.Repository)
                .WithMany(c => c.Administrators)
                .HasForeignKey(bc => bc.RepositoryId);
        }
    }
}
