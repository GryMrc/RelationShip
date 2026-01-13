using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Infra.EntityTypeConfigurations;

public class UserProfileAnswerConfiguration : IEntityTypeConfiguration<UserProfileAnswer>
{
    public void Configure(EntityTypeBuilder<UserProfileAnswer> builder)
    {
        builder.ToTable("user_profile_answers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.UserProfileId).HasColumnName("user_profile_id").IsRequired();
        builder.Property(x => x.QuestionAnswerId).HasColumnName("question_answer_id").IsRequired();

        builder.HasOne(x => x.UserProfile)
            .WithMany(x => x.UserProfileAnswers)
            .HasForeignKey(x => x.UserProfileId);

        builder.HasOne(x => x.QuestionAnswer)
            .WithMany(x => x.UserProfileAnswers)
            .HasForeignKey(x => x.QuestionAnswerId);
    }
}
