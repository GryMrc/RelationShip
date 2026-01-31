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

        builder.Property(x => x.SwiperUserId).HasColumnName("swiper_user_id").IsRequired();
        builder.Property(x => x.SwipedUserId).HasColumnName("swiped_user_id").IsRequired();
        builder.Property(x => x.SwipeType).HasColumnName("swipe_type").IsRequired();

        // Indexes for performance
        builder.HasIndex(x => new { x.SwiperUserId, x.SwipedUserId }).IsUnique().HasDatabaseName("idx_swipes_swiper_swiped_unique");
        builder.HasIndex(x => new { x.SwipedUserId, x.SwiperUserId }).HasDatabaseName("idx_swipes_swiped_swiper");
    }
}
