using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Infra.EntityTypeConfigurations;

public class QuestionAnswerConfiguration : IEntityTypeConfiguration<QuestionAnswer>
{
    public void Configure(EntityTypeBuilder<QuestionAnswer> builder)
    {
        builder.ToTable("question_answers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.QuestionId).HasColumnName("question_id").IsRequired();
        builder.Property(x => x.AnswerText).HasColumnName("answer_text").HasMaxLength(500).IsRequired();

        builder.HasMany(x => x.UserProfileAnswers)
            .WithOne(x => x.QuestionAnswer)
            .HasForeignKey(x => x.QuestionAnswerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
