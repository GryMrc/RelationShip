using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Models.Profile.Responses;

public class ProfileResponse
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? Bio { get; set; }
    public Gender Gender { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime DateOfBirth { get; set; }
    public byte? Height { get; set; }
    public byte? Weight { get; set; }
    public ZodiacSign? ZodiacSign { get; set; }
    public ZodiacSign? RisingZodiacSign { get; set; }
    public bool IsVerified { get; set; }
    public Gender InterestedInGender { get; set; }
    public sbyte MaxDistancePreference { get; set; }
    public byte MinAgePreference { get; set; }
    public byte MaxAgePreference { get; set; }
    public List<Hobby.Responses.HobbyResponse> Hobbies { get; set; }
    public List<ProfilePhotoResponse> Photos { get; set; }
    public List<ProfileAnswerResponse> Answers { get; set; }
}
