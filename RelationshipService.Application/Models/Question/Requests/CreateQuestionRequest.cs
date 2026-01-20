namespace RelationshipService.Application.Models.Question.Requests;

public class CreateQuestionRequest
{
    public string Text { get; set; }
    public List<string> Answers { get; set; } = new();
}
