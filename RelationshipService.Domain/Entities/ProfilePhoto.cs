namespace RelationshipService.Domain.Entities;

public class ProfilePhoto
{
    public int Id { get; set; }
    public int UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; }
    public string PhotoUrl { get; set; }
    public bool IsMain { get; set; }
    public int OrderNo { get; set; }
}
