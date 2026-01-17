using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Infra.EntityTypeConfigurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Bio).HasColumnName("bio").HasMaxLength(1000);
        builder.Property(x => x.Gender).HasColumnName("gender").IsRequired();
        builder.Property(x => x.DateOfBirth).HasColumnName("date_of_birth").IsRequired();
        builder.Property(x => x.Height).HasColumnName("height");
        builder.Property(x => x.Weight).HasColumnName("weight");
        builder.Property(x => x.ZodiacSign).HasColumnName("zodiac_sign").HasMaxLength(50);
        builder.Property(x => x.RisingZodiacSign).HasColumnName("rising_zodiac_sign").HasMaxLength(50);

        // Relationships are primarily configured in UserProfileLocationConfiguration and UserProfilePreferencesConfiguration
        // for better clarity in 1:1 bidirectional mapping.

        // Relationships
        builder.HasMany(x => x.ProfilePhotos)
            .WithOne(x => x.UserProfile)
            .HasForeignKey(x => x.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.UserProfileAnswers)
            .WithOne(x => x.UserProfile)
            .HasForeignKey(x => x.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Hobbies)
            .WithMany()
            .UsingEntity(j => j.ToTable("user_profile_hobbies"));
    }
}
