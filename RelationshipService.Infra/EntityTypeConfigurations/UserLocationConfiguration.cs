using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Infra.EntityTypeConfigurations;

public class UserLocationConfiguration : IEntityTypeConfiguration<UserLocation>
{
    public void Configure(EntityTypeBuilder<UserLocation> builder)
    {
        builder.ToTable("user_locations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.UserProfileId).HasColumnName("user_profile_id").IsRequired();
        builder.Property(x => x.Latitude).HasColumnName("latitude").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Longitude).HasColumnName("longitude").HasMaxLength(50).IsRequired();

        builder.HasOne(x => x.UserProfile)
            .WithOne(x => x.Location)
            .HasForeignKey<UserLocation>(x => x.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
