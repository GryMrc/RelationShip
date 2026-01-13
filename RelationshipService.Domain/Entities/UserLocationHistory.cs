using RelationshipService.Domain.Base;

namespace RelationshipService.Domain.Entities;

public class UserLocationHistory : Entity<int>
{
    public int UserId { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
}
