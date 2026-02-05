namespace RelationshipService.Application.Models.Profile.Responses;

public class ProfileAnswerResponse
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string QuestionText { get; set; }
    public int AnswerId { get; set; }
    public string AnswerText { get; set; }
}
