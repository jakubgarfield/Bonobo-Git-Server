using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            SetPrimaryKey(builder);
            SetProperties(builder);
            SetTableAndColumnMappings(builder);
        }

        private void SetTableAndColumnMappings(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("User");
            builder.Property(t => t.Id).HasColumnName("Id");
            builder.Property(t => t.GivenName).HasColumnName("Name");
            builder.Property(t => t.Surname).HasColumnName("Surname");
            builder.Property(t => t.Username).HasColumnName("Username");
            builder.Property(t => t.Password).HasColumnName("Password");
            builder.Property(t => t.PasswordSalt).HasColumnName("PasswordSalt");
            builder.Property(t => t.Email).HasColumnName("Email");

            //builder.HasMany(u => u.AdministratedRepositories)
            //    .WithOne(u => u.User)
            //    .HasForeignKey(u => u.UserId);

            //builder.HasMany(u => u.Repositories)
            //    .WithOne(u => u.User)
            //    .HasForeignKey(u => u.UserId);

            //builder.HasMany(u => u.Teams)
            //    .WithOne(u => u.User)
            //    .HasForeignKey(u => u.UserId);

            //builder.HasMany(u => u.AdministratedRepositories)
            //    .WithOne(u => u.User)
            //    .HasForeignKey(u => u.UserId);
        }

        private void SetProperties(EntityTypeBuilder<User> builder)
        {
            builder.Property(t => t.GivenName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(t => t.Surname)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(t => t.Username)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(t => t.Password)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(t => t.Email)
                .IsRequired()
                .HasMaxLength(255);
        }

        private void SetPrimaryKey(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(t => t.Id);
        }
    }
}
