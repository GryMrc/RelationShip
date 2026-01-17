using RelationshipService.Domain.Base;

namespace RelationshipService.Domain.Entities;

public class UserLocation : Entity<int>
{
    public int UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
}
