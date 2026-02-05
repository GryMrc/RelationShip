using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Infra.EntityTypeConfigurations;

public class UserProfilePhotoConfiguration : IEntityTypeConfiguration<ProfilePhoto>
{
    public void Configure(EntityTypeBuilder<ProfilePhoto> builder)
    {
        builder.ToTable("profile_photos");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ProfileId).HasColumnName("profile_id").IsRequired();
        builder.Property(x => x.PhotoUrl).HasColumnName("photo_url").HasMaxLength(1000).IsRequired();
        builder.Property(x => x.IsMain).HasColumnName("is_main").IsRequired();
        builder.Property(x => x.Order).HasColumnName("order").IsRequired();
    }
}
