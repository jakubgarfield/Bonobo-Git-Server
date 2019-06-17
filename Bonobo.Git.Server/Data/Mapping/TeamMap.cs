using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class TeamMap : IEntityTypeConfiguration<Team>
    {
        private readonly ValueConverter _primaryKeyConverter;

        public TeamMap(ValueConverter primaryKeyConverter)
        {
            _primaryKeyConverter = primaryKeyConverter;
        }

        public void Configure(EntityTypeBuilder<Team> builder)
        {
            SetPrimaryKey(builder);
            SetProperties(builder);
            SetTableAndColumnMappings(builder);
            SetRelationships(builder);
        }

        private void SetRelationships(EntityTypeBuilder<Team> builder)
        {
            //builder.HasMany(t => t.Users)
            //    .WithMany(t => t.Teams)
            //    .Map(m =>
            //    {
            //        m.ToTable("UserTeam_Member");
            //        m.MapLeftKey("Team_Id");
            //        m.MapRightKey("User_Id");
            //    });
        }

        private void SetTableAndColumnMappings(EntityTypeBuilder<Team> builder)
        {
            builder.ToTable("Team");
            builder.Property(t => t.Id).HasColumnName("Id").HasConversion(_primaryKeyConverter);
            builder.Property(t => t.Name).HasColumnName("Name");
            builder.Property(t => t.Description).HasColumnName("Description");
        }

        private void SetProperties(EntityTypeBuilder<Team> builder)
        {
            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(t => t.Description)
                .HasMaxLength(255);
        }

        private void SetPrimaryKey(EntityTypeBuilder<Team> builder)
        {
            builder.HasKey(t => t.Id);
        }
    }
}
