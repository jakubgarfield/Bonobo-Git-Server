using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class TeamMap : EntityTypeConfiguration<Team>
    {
        public TeamMap()
        {
            SetPrimaryKey();
            SetProperties();
            SetTableAndColumnMappings();
            SetRelationships();
        }


        private void SetRelationships()
        {
            HasMany(t => t.Users)
                .WithMany(t => t.Teams)
                .Map(m =>
                {
                    m.ToTable("UserTeam_Member");
                    m.MapLeftKey("Team_Name");
                    m.MapRightKey("User_Username");
                });
        }

        private void SetTableAndColumnMappings()
        {
            ToTable("Team");
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
