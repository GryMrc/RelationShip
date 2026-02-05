namespace RelationshipService.Application.Models.Profile.Responses;

public class ProfilePhotoResponse
{
    public int Id { get; set; }
    public string PhotoUrl { get; set; }
    public bool IsMain { get; set; }
    public int Order { get; set; }
    public bool IsBlurred { get; set; }
}
