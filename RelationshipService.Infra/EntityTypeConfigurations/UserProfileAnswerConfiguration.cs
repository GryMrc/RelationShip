using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Infra.EntityTypeConfigurations;

public class UserProfileAnswerConfiguration : IEntityTypeConfiguration<ProfileAnswer>
{
    public void Configure(EntityTypeBuilder<ProfileAnswer> builder)
    {
        builder.ToTable("profile_answers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ProfileId).HasColumnName("profile_id").IsRequired();
        builder.Property(x => x.QuestionAnswerId).HasColumnName("question_answer_id").IsRequired();

        builder.HasOne(x => x.Profile)
            .WithMany(x => x.Answers)
            .HasForeignKey(x => x.ProfileId);

        builder.HasOne(x => x.QuestionAnswer)
            .WithMany(x => x.ProfileAnswers)
            .HasForeignKey(x => x.QuestionAnswerId);
    }
}
