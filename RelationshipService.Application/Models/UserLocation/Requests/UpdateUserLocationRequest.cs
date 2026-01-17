using System.Text.Json.Serialization;

namespace RelationshipService.Application.Models.UserLocation.Requests;

public class UpdateUserLocationRequest
{
    [JsonIgnore]
    public int UserId { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
}
