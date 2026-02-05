using System.Text.Json.Serialization;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Models.Profile.Requests;

public class UpdateProfilePreferencesRequest
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    public Gender InterestedInGender { get; set; }
    public sbyte MaxDistancePreference { get; set; }
    public byte MinAgePreference { get; set; }
    public byte MaxAgePreference { get; set; }
    public Mode Mode { get; set; }
}


