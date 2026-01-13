using RelationshipService.Domain.Base;

namespace RelationshipService.Domain.Entities;
public class QuestionAnswer : Entity<int>
{
    public int QuestionId { get; private set; }
    public Question Question { get; set; }

    public string AnswerText { get; private set; }

    public List<UserProfileAnswer> UserProfileAnswers { get; set; }
}