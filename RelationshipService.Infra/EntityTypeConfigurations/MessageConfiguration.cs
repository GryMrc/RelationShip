using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Infra.EntityTypeConfigurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("messages");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");

            builder.Property(x => x.MatchId)
                .HasColumnName("match_id")
                .IsRequired();

            builder.Property(x => x.SenderProfileId)
                .HasColumnName("sender_profile_id")
                .IsRequired();

            builder.Property(x => x.ReceiverProfileId)
                .HasColumnName("receiver_profile_id")
                .IsRequired();

            builder.Property(x => x.Content)
                .HasColumnName("content")
                .IsRequired()
                .HasMaxLength(2000);

            builder.HasOne(x => x.Match)
                .WithMany()
                .HasForeignKey(x => x.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SenderProfile)
                .WithMany()
                .HasForeignKey(x => x.SenderProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReceiverProfile)
                .WithMany()
                .HasForeignKey(x => x.ReceiverProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
