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

        builder.Property(x => x.SwiperProfilId).HasColumnName("swiper_user_id").IsRequired();
        builder.Property(x => x.SwipedProfilId).HasColumnName("swiped_user_id").IsRequired();
        builder.Property(x => x.SwipeType).HasColumnName("swipe_type").IsRequired();
        builder.Property(x => x.Mode).HasColumnName("mode").IsRequired();

        // Indexes for performance
        builder.HasIndex(x => new { x.SwiperProfilId, x.SwipedProfilId, x.Mode }).IsUnique().HasDatabaseName("idx_swipes_swiper_swiped_mode_unique");
        builder.HasIndex(x => new { x.SwipedProfilId, x.SwiperProfilId, x.Mode }).HasDatabaseName("idx_swipes_swiped_swiper_mode");
    }
}
