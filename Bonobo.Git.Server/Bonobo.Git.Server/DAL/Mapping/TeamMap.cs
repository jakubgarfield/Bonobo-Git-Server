using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Bonobo.Git.Server.DAL.Mapping
{
    public class TeamMap : EntityTypeConfiguration<Team>
    {
        public TeamMap()
        {
            // Primary Key
            this.HasKey(t => t.Name);

            // Properties
            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.Description)
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("Team");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Description).HasColumnName("Description");

            // Relationships
            this.HasMany(t => t.Users)
                .WithMany(t => t.Teams)
                .Map(m =>
                    {
                        m.ToTable("UserTeam_Member");
                        m.MapLeftKey("Team_Name");
                        m.MapRightKey("User_Username");
                    });


        }
    }
}
