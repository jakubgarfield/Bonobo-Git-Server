using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class TeamRepositoryPermissionMap : IEntityTypeConfiguration<TeamRepositoryPermission>
    {
        public void Configure(EntityTypeBuilder<TeamRepositoryPermission> builder)
        {
            builder.ToTable("TeamRepository_Permission");
            builder.HasKey(ur => new { ur.RepositoryId, ur.TeamId });

            builder.Property(t => t.TeamId).HasColumnName("Team_Id").IsRequired();
            builder.Property(t => t.RepositoryId).HasColumnName("Repository_Id").IsRequired();

            builder
                .HasOne(bc => bc.Team)
                .WithMany(c => c.Repositories)
                .HasForeignKey(bc => bc.TeamId);

            builder
                .HasOne(bc => bc.Repository)
                .WithMany(b => b.Teams)
                .HasForeignKey(bc => bc.RepositoryId);
        }
    }
}
