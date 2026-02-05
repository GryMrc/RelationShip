namespace RelationshipService.Domain.Entities;

public class ProfileHobby
{
    public long ProfileId { get; set; }
    public Profile Profile { get; set; }
    public int HobbyId { get; set; }
    public Hobby Hobby { get; set; }
}
