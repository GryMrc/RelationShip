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

        builder.Property(x => x.ProfileAId).HasColumnName("user_a_id").IsRequired();
        builder.Property(x => x.ProfileBId).HasColumnName("user_b_id").IsRequired();
        builder.Property(x => x.Mode).HasColumnName("mode").IsRequired();
        builder.Property(x => x.MatchStatus).HasColumnName("match_status").IsRequired();
        builder.Property(x => x.FreezedUserId).HasColumnName("freezed_user_id");
        builder.Property(x => x.DeletedUserId).HasColumnName("deleted_user_id");
        builder.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(500);

        // Unique constraint to prevent duplicate matches between same two users in the same mode
        builder.HasIndex(x => new { x.ProfileAId, x.ProfileBId, x.Mode }).IsUnique().HasDatabaseName("idx_matches_a_b_mode_unique");
    }
}
