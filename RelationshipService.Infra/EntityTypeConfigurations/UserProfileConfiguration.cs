using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RelationshipService.Domain.Entities;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Infra.EntityTypeConfigurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("profiles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Bio).HasColumnName("bio").HasMaxLength(1000);
        builder.Property(x => x.Gender).HasColumnName("gender").IsRequired();
        builder.Property(x => x.DateOfBirth).HasColumnName("date_of_birth").IsRequired();
        builder.Property(x => x.Location).HasColumnName("location").HasColumnType("geometry(Point, 4326)").IsRequired();
        builder.Property(x => x.Height).HasColumnName("height");
        builder.Property(x => x.Weight).HasColumnName("weight");
        builder.Property(x => x.ZodiacSign).HasColumnName("zodiac_sign");
        builder.Property(x => x.RisingZodiacSign).HasColumnName("rising_zodiac_sign");
        builder.Property(x => x.IsVerified).HasColumnName("is_verified").HasDefaultValue(false);
        builder.Property(x => x.Mode).HasColumnName("mode").HasDefaultValue(Mode.Date).IsRequired();
        builder.Property(x => x.SubscriptionPlan).HasColumnName("subscription_plan").HasDefaultValue(SubscriptionPlan.Free).IsRequired();

        // Spatial Index (GIST) for high-performance location queries
        builder.HasIndex(x => x.Location).HasMethod("GIST");

        // Partial Index for active users (excluding deleted ones from index for speed)
        builder.HasIndex(x => x.IsDeleted)
               .HasFilter("is_deleted = false")
               .HasDatabaseName("IX_UserProfile_ActiveUsers");

        // Composite Index for Discovery (Mode + Gender + IsDeleted optimization)
        builder.HasIndex(x => new { x.Mode, x.Gender, x.IsDeleted })
               .HasDatabaseName("IX_UserProfile_Discovery_BasicFilter");

        // Index for Age filtering (Range queries)
        builder.HasIndex(x => x.DateOfBirth)
               .HasDatabaseName("IX_UserProfile_AgeFilter");

        builder.HasMany(x => x.Photos)
            .WithOne(x => x.Profile)
            .HasForeignKey(x => x.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Answers)
            .WithOne(x => x.Profile)
            .HasForeignKey(x => x.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Devices)
            .WithOne(x => x.Profile)
            .HasForeignKey(x => x.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Hobbies)
            .WithMany()
            .UsingEntity<ProfileHobby>(
                j =>
                {
                    j.ToTable("profile_hobbies");
                    j.HasKey(t => new { t.ProfileId, t.HobbyId });
                    j.Property(x => x.ProfileId).HasColumnName("profile_id");
                    j.Property(x => x.HobbyId).HasColumnName("hobby_id");
                });
    }
}
