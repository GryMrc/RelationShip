namespace RelationshipService.Domain.Entities;

public class ProfileAnswer
{
    public int Id { get; set; }

    public long ProfileId { get; set; }

    public Profile Profile { get; set; }

    public int QuestionAnswerId { get; set; }

    public QuestionAnswer QuestionAnswer { get; set; }
}