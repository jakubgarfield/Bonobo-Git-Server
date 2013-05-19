using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class UserMap : EntityTypeConfiguration<User>
    {
        public UserMap()
        {
            SetPrimaryKey();
            SetProperties();
            SetTableAndColumnMappings();
        }


        private void SetTableAndColumnMappings()
        {
            ToTable("User");
            Property(t => t.Name).HasColumnName("Name");
            Property(t => t.Surname).HasColumnName("Surname");
            Property(t => t.Username).HasColumnName("Username");
            Property(t => t.Password).HasColumnName("Password");
            Property(t => t.Email).HasColumnName("Email");
        }

        private void SetProperties()
        {
            Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.Surname)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.Username)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.Password)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.Email)
                .IsRequired()
                .HasMaxLength(255);
        }

        private void SetPrimaryKey()
        {
            HasKey(t => t.Username);
        }
    }
}
