namespace RelationshipService.Application.Models.UserProfile.Responses;

public class UserProfileAnswerResponse
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string QuestionText { get; set; }
    public int AnswerId { get; set; }
    public string AnswerText { get; set; }
}
