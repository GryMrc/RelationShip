using RelationshipService.Domain.Base;

namespace RelationshipService.Domain.Entities;

public class ProfilePhoto : Entity<int>
{
    public long ProfileId { get; set; }
    public Profile Profile { get; set; }
    public string PhotoUrl { get; set; }
    public bool IsMain { get; set; }
    public int Order { get; set; }
}
