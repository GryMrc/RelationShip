using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Models.UserProfile.Responses;

public class UserProfileResponse
{
    public string Name { get; set; }
    public string Bio { get; set; }
    public Gender Gender { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public DateTime DateOfBirth { get; set; }
    public byte? Height { get; set; }
    public byte? Weight { get; set; }
    public string ZodiacSign { get; set; }
    public string RisingZodiacSign { get; set; }
    public Gender InterestedInGender { get; set; }
    public sbyte MaxDistancePreference { get; set; }
    public byte MinAgePreference { get; set; }
    public byte MaxAgePreference { get; set; }
}
