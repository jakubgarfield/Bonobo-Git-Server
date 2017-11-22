using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class UserTeamMemberMap : IEntityTypeConfiguration<UserTeamMember>
    {
        public void Configure(EntityTypeBuilder<UserTeamMember> builder)
        {
            builder.ToTable("UserTeam_Member");
            builder.HasKey(ur => new { ur.UserId, ur.TeamId });

            builder.Property(t => t.UserId).HasColumnName("User_Id").IsRequired();
            builder.Property(t => t.TeamId).HasColumnName("Team_Id").IsRequired();

            builder
                .HasOne(bc => bc.User)
                .WithMany(b => b.Teams)
                .HasForeignKey(bc => bc.UserId);

            builder
                .HasOne(bc => bc.Team)
                .WithMany(c => c.Users)
                .HasForeignKey(bc => bc.TeamId);
        }
    }
}
