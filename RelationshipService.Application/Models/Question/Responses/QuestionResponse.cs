namespace RelationshipService.Application.Models.Question.Responses;

public class QuestionResponse
{
    public int Id { get; set; }
    public string Text { get; set; }
    public List<AnswerResponse> Answers { get; set; } = new();
}
