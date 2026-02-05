using RelationshipService.Domain.Base;

namespace RelationshipService.Domain.Entities;

public class ProfileLocationHistory : Entity<int>
{
    public long ProfileId { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
}
