using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Infra.EntityTypeConfigurations;

public class UserLocationHistoryConfiguration : IEntityTypeConfiguration<ProfileLocationHistory>
{
    public void Configure(EntityTypeBuilder<ProfileLocationHistory> builder)
    {
        builder.ToTable("profile_location_histories");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ProfileId).HasColumnName("profile_id").IsRequired();
        builder.Property(x => x.Latitude).HasColumnName("latitude").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Longitude).HasColumnName("longitude").HasMaxLength(50).IsRequired();
    }
}
