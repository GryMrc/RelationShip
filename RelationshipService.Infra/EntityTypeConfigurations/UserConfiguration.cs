using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RelationshipService.Domain.Entities;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Infra.EntityTypeConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).HasColumnName("id");

            builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
            builder.Property(u => u.Email).HasColumnName("email");
            builder.Property(u => u.Provider).IsRequired().HasMaxLength(50);
            builder.Property(u => u.Provider).HasColumnName("provider");
            builder.Property(u => u.ProviderKey).IsRequired().HasMaxLength(256);
            builder.Property(u => u.ProviderKey).HasColumnName("provider_key");

            builder.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue(UserRole.User)
                .HasConversion<string>()
                .HasColumnName("role");

            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => new { u.Provider, u.ProviderKey }).IsUnique();

            builder.HasOne(u => u.Profile)
                .WithOne()
                .HasForeignKey<Profile>(p => p.UserId);
        }
    }
}
