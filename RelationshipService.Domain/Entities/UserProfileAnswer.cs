namespace RelationshipService.Domain.Entities;

public class UserProfileAnswer
{
    public int Id { get; set; }

    public long UserProfileId { get; set; }

    public UserProfile UserProfile { get; set; }

    public int QuestionAnswerId { get; set; }

    public QuestionAnswer QuestionAnswer { get; set; }
}