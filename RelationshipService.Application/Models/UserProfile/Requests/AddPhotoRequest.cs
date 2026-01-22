namespace RelationshipService.Application.Models.UserProfile.Requests;

public class AddPhotoRequest
{
    public string PhotoUrl { get; set; }
    public bool IsMain { get; set; }
    public int Order { get; set; }
}
