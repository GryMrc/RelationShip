namespace RelationshipService.Application.Models.Profile.Requests;

public class AddProfilePhotoRequest
{
    public string PhotoUrl { get; set; }
    public bool IsMain { get; set; }
    public int Order { get; set; }
}
