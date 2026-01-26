using System.Security.Cryptography;
using System.Text;
using RelationshipService.Application.Models.Hobby.Responses;
using RelationshipService.Application.Models.UserProfile.Responses;
using RelationshipService.Domain.Entities;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Mappers;

public static class UserProfileMapper
{
    private const string SecretKey = "your-very-secure-secret-key-change-me-later";

    public static UserProfileResponse ToResponse(this UserProfile profile)
    {
        return new UserProfileResponse
        {
            UserId = profile.UserId,
            Name = profile.Name,
            Bio = profile.Bio,
            Gender = profile.Gender,
            Latitude = profile.Location.Y,
            Longitude = profile.Location.X,
            DateOfBirth = profile.DateOfBirth,
            Height = profile.Height,
            Weight = profile.Weight,
            ZodiacSign = profile.ZodiacSign,
            RisingZodiacSign = profile.RisingZodiacSign,
            IsVerified = profile.IsVerified,
            InterestedInGender = profile.Preferences?.InterestedInGender ?? (Gender)0,
            MaxDistancePreference = profile.Preferences?.MaxDistancePreference ?? 0,
            MinAgePreference = profile.Preferences?.MinAgePreference ?? 0,
            MaxAgePreference = profile.Preferences?.MaxAgePreference ?? 0,
            Hobbies = profile.Hobbies?.Select(h => new HobbyResponse
            {
                Id = h.Id,
                Name = h.Name
            }).ToList() ?? new List<HobbyResponse>(),
            ProfilePhotos = profile.ProfilePhotos?.Select(p => new UserProfilePhotoResponse
            {
                Id = p.Id,
                PhotoUrl = p.PhotoUrl,
                IsMain = p.IsMain,
                Order = p.Order
            }).ToList() ?? new List<UserProfilePhotoResponse>(),
            UserProfileAnswers = profile.UserProfileAnswers?.Select(a => new UserProfileAnswerResponse
            {
                Id = a.Id,
                QuestionId = a.QuestionAnswer?.QuestionId ?? 0,
                QuestionText = a.QuestionAnswer?.Question?.Text ?? string.Empty,
                AnswerId = a.QuestionAnswerId,
                AnswerText = a.QuestionAnswer?.AnswerText ?? string.Empty
            }).ToList() ?? new List<UserProfileAnswerResponse>()
        };
    }

    public static DiscoveryProfileResponse ToDiscoveryResponse(this UserProfile profile, int swiperId)
    {
        var response = new DiscoveryProfileResponse
        {
            UserId = profile.UserId,
            Name = profile.Name,
            Bio = profile.Bio,
            Gender = profile.Gender,
            Latitude = profile.Location.Y,
            Longitude = profile.Location.X,
            DateOfBirth = profile.DateOfBirth,
            Height = profile.Height,
            Weight = profile.Weight,
            ZodiacSign = profile.ZodiacSign,
            RisingZodiacSign = profile.RisingZodiacSign,
            IsVerified = profile.IsVerified,
            InterestedInGender = profile.Preferences?.InterestedInGender ?? (Gender)0,
            MaxDistancePreference = profile.Preferences?.MaxDistancePreference ?? 0,
            MinAgePreference = profile.Preferences?.MinAgePreference ?? 0,
            MaxAgePreference = profile.Preferences?.MaxAgePreference ?? 0,
            Hobbies = profile.Hobbies?.Select(h => new HobbyResponse
            {
                Id = h.Id,
                Name = h.Name
            }).ToList() ?? new List<HobbyResponse>(),
            ProfilePhotos = profile.ProfilePhotos?.Select(p => new UserProfilePhotoResponse
            {
                Id = p.Id,
                PhotoUrl = p.PhotoUrl,
                IsMain = p.IsMain,
                Order = p.Order
            }).ToList() ?? new List<UserProfilePhotoResponse>(),
            UserProfileAnswers = profile.UserProfileAnswers?.Select(a => new UserProfileAnswerResponse
            {
                Id = a.Id,
                QuestionId = a.QuestionAnswer?.QuestionId ?? 0,
                QuestionText = a.QuestionAnswer?.Question?.Text ?? string.Empty,
                AnswerId = a.QuestionAnswerId,
                AnswerText = a.QuestionAnswer?.AnswerText ?? string.Empty
            }).ToList() ?? new List<UserProfileAnswerResponse>()
        };

        response.DiscoveryToken = GenerateDiscoveryToken(swiperId, profile.UserId);
        return response;
    }

    private static string GenerateDiscoveryToken(int swiperId, int swipedId)
    {
        var expiry = DateTime.UtcNow.AddHours(24).Ticks;
        var payload = $"{swiperId}:{swipedId}:{expiry}";
        var key = Encoding.UTF8.GetBytes(SecretKey);

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var signature = Convert.ToBase64String(hash);

        return $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(payload))}.{signature}";
    }
}
