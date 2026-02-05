using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Infra.EntityTypeConfigurations;

public class SwipeConfiguration : IEntityTypeConfiguration<Swipe>
{
    public void Configure(EntityTypeBuilder<Swipe> builder)
    {
        builder.ToTable("swipes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.SwiperProfileId).HasColumnName("swiper_profile_id").IsRequired();
        builder.Property(x => x.SwipedProfileId).HasColumnName("swiped_profile_id").IsRequired();
        builder.Property(x => x.SwipeType).HasColumnName("swipe_type").IsRequired();
        builder.Property(x => x.Mode).HasColumnName("mode").IsRequired();

        // Indexes for performance
        builder.HasIndex(x => new { x.SwiperProfileId, x.SwipedProfileId, x.Mode }).IsUnique().HasDatabaseName("idx_swipes_swiper_swiped_mode_unique");
        builder.HasIndex(x => new { x.SwipedProfileId, x.SwiperProfileId, x.Mode }).HasDatabaseName("idx_swipes_swiped_swiper_mode");
    }
}
