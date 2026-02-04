using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Infra.EntityTypeConfigurations;

public class UserLocationHistoryConfiguration : IEntityTypeConfiguration<UserLocationHistory>
{
    public void Configure(EntityTypeBuilder<UserLocationHistory> builder)
    {
        builder.ToTable("user_location_histories");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.UserProfileId).HasColumnName("user_profil_id").IsRequired();
        builder.Property(x => x.Latitude).HasColumnName("latitude").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Longitude).HasColumnName("longitude").HasMaxLength(50).IsRequired();
    }
}
