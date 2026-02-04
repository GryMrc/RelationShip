using System.Text.Json.Serialization;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Models.UserProfile.Requests;

public abstract class UserProfileRequestBase
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    public string Bio { get; set; }
    public byte? Height { get; set; }
    public byte? Weight { get; set; }
    public ZodiacSign? ZodiacSign { get; set; }
    public ZodiacSign? RisingZodiacSign { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
