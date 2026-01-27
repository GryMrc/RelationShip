using System.Text.Json.Serialization;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Models.UserPrefences.Requests;

public class UpdateUserPreferencesRequest
{
    [JsonIgnore]
    public int UserId { get; set; }
    public Gender InterestedInGender { get; set; }
    public sbyte MaxDistancePreference { get; set; }
    public byte MinAgePreference { get; set; }
    public byte MaxAgePreference { get; set; }
    public MatchMode MatchMode { get; set; }
}


