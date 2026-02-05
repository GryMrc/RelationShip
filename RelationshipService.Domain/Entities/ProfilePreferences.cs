using RelationshipService.Domain.Base;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Domain.Entities;

public class ProfilePreferences : Entity<int>
{
    public long ProfileId { get; set; }
    public Profile Profile { get; set; }
    public Gender InterestedInGender { get; set; }
    public sbyte MaxDistancePreference { get; set; }
    public byte MinAgePreference { get; set; }
    public byte MaxAgePreference { get; set; }
}


