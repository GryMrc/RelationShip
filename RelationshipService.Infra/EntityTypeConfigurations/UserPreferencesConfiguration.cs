using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Infra.EntityTypeConfigurations;

public class UserPreferencesConfiguration : IEntityTypeConfiguration<UserPreferences>
{
    public void Configure(EntityTypeBuilder<UserPreferences> builder)
    {
        builder.ToTable("user_preferences");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.UserProfileId).HasColumnName("user_profile_id").IsRequired();
        builder.Property(x => x.InterestedInGender).HasColumnName("interested_in_gender").IsRequired();
        builder.Property(x => x.MaxDistancePreference).HasColumnName("max_distance_preference");
        builder.Property(x => x.MinAgePreference).HasColumnName("min_age_preference");
        builder.Property(x => x.MaxAgePreference).HasColumnName("max_age_preference");



        builder.HasOne(x => x.UserProfile)
            .WithOne(x => x.Preferences)
            .HasForeignKey<UserPreferences>(x => x.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
