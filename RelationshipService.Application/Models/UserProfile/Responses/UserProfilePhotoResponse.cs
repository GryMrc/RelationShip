namespace RelationshipService.Application.Models.UserProfile.Responses;

public class UserProfilePhotoResponse
{
    public int Id { get; set; }
    public string PhotoUrl { get; set; }
    public bool IsMain { get; set; }
    public int Order { get; set; }
    public bool IsBlurred { get; set; }
}
