using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class TeamMap : IEntityTypeConfiguration<Team>
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            SetPrimaryKey(builder);
            SetProperties(builder);
            SetTableAndColumnMappings(builder);
            SetRelationships(builder);
        }

        private void SetRelationships(EntityTypeBuilder<Team> builder)
        {
            builder.HasMany(t => t.Users)
                .WithOne(t => t.Team)
                .HasForeignKey(t => t.TeamId);

            builder.HasMany(t => t.Repositories)
                .WithOne(t => t.Team)
                .HasForeignKey(t => t.TeamId);
        }

        private void SetTableAndColumnMappings(EntityTypeBuilder<Team> builder)
        {
            builder.ToTable("Team");
            builder.Property(t => t.Id).HasColumnName("Id");
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
