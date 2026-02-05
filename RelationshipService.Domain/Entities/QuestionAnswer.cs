using RelationshipService.Domain.Base;

namespace RelationshipService.Domain.Entities;
public class QuestionAnswer : Entity<int>
{
    public int QuestionId { get; set; }
    public Question Question { get; set; }

    public string AnswerText { get; set; }

    public List<ProfileAnswer> ProfileAnswers { get; set; }
}