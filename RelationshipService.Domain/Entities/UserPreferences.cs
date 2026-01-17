using RelationshipService.Domain.Base;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Domain.Entities;

public class UserPreferences : Entity<int>
{
    public int UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; }
    public Gender InterestedInGender { get; set; }
    public sbyte MaxDistancePreference { get; set; }
    public byte MinAgePreference { get; set; }
    public byte MaxAgePreference { get; set; }
}
