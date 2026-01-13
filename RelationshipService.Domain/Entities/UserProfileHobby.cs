namespace RelationshipService.Domain.Entities;

public class UserProfileHobby
{
    public int UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; }
    public int HobbyId { get; set; }
    public Hobby Hobby { get; set; }
}
