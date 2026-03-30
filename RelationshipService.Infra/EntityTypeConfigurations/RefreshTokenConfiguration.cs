using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Infra.EntityTypeConfigurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_tokens");
            builder.HasKey(rt => rt.Id);
            builder.Property(rt => rt.Id).HasColumnName("id");

            builder.Property(rt => rt.Token).IsRequired().HasMaxLength(256).HasColumnName("token");
            builder.Property(rt => rt.ReplacedByToken).HasMaxLength(256).HasColumnName("replaced_by_token");

            builder.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId);

            builder.HasIndex(rt => rt.Token).IsUnique();
        }
    }
}
