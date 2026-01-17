using System.Text.Json.Serialization;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Models.UserProfile.Requests;

public abstract class UserProfileRequestBase
{
    [JsonIgnore]
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Bio { get; set; }
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public byte? Height { get; set; }
    public byte? Weight { get; set; }
    public string ZodiacSign { get; set; }
    public string RisingZodiacSign { get; set; }
}
