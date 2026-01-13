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
        builder.Property(x => x.IsLiked).HasColumnName("is_liked").IsRequired();
    }
}
