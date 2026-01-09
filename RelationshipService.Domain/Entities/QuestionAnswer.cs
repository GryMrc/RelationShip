namespace RelationshipService.Domain.Entities;
public class QuestionAnswer
{
    public int Id { get; private set; }

    public int QuestionId { get; private set; }

    public string AnswerText { get; private set; }

    public QuestionAnswer(int id, int questionId, string answerText)
    {
        Id = id;
        QuestionId = questionId;
        AnswerText = answerText;
    }
}