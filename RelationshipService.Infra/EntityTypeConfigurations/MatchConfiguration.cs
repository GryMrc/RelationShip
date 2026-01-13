using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Infra.EntityTypeConfigurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("matches");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.UserAId).HasColumnName("user_a_id").IsRequired();
        builder.Property(x => x.UserBId).HasColumnName("user_b_id").IsRequired();
        builder.Property(x => x.MatchMode).HasColumnName("match_mode").IsRequired();
        builder.Property(x => x.MatchStatus).HasColumnName("match_status").IsRequired();
        builder.Property(x => x.FreezedUserId).HasColumnName("freezed_user_id");
        builder.Property(x => x.DeletedUserId).HasColumnName("deleted_user_id");
        builder.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(500);
    }
}
