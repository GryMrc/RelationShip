using RelationshipService.Domain.Base;

namespace RelationshipService.Domain.Entities;

public class UserProfilePhoto : Entity<int>
{
    public int UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; }
    public string PhotoUrl { get; set; }
    public bool IsMain { get; set; }
    public int Order { get; set; }
}
