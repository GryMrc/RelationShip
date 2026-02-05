using System.Security.Cryptography;
using System.Text;
using RelationshipService.Application.Models.Hobby.Responses;
using RelationshipService.Application.Models.UserProfile.Responses;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Domain.Entities;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Mappers;

public static class UserProfileMapper
{
    public static UserProfileResponse ToResponse(this Profile profile)
    {
        return new UserProfileResponse
        {
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
            ProfilePhotos = profile.Photos?.Select(p => new UserProfilePhotoResponse
            {
                Id = p.Id,
                PhotoUrl = p.PhotoUrl,
                IsMain = p.IsMain,
                Order = p.Order
            }).ToList() ?? new List<UserProfilePhotoResponse>(),
            UserProfileAnswers = profile.Answers?.Select(a => new UserProfileAnswerResponse
            {
                Id = a.Id,
                QuestionId = a.QuestionAnswer?.QuestionId ?? 0,
                QuestionText = a.QuestionAnswer?.Question?.Text ?? string.Empty,
                AnswerId = a.QuestionAnswerId,
                AnswerText = a.QuestionAnswer?.AnswerText ?? string.Empty
            }).ToList() ?? new List<UserProfileAnswerResponse>()
        };
    }

    public static DiscoveryProfileResponse ToDiscoveryResponse(this Profile profile, Guid swiperId, IDiscoveryTokenService tokenService)
    {
        var response = profile.ToResponse(); // Start with base mapping
        
        var discoveryResponse = new DiscoveryProfileResponse
        {
            Id = response.Id,
            Name = response.Name,
            Bio = response.Bio,
            Gender = response.Gender,
            Latitude = response.Latitude,
            Longitude = response.Longitude,
            DateOfBirth = response.DateOfBirth,
            Height = response.Height,
            Weight = response.Weight,
            ZodiacSign = response.ZodiacSign,
            RisingZodiacSign = response.RisingZodiacSign,
            IsVerified = response.IsVerified,
            InterestedInGender = response.InterestedInGender,
            MaxDistancePreference = response.MaxDistancePreference,
            MinAgePreference = response.MinAgePreference,
            MaxAgePreference = response.MaxAgePreference,
            Hobbies = response.Hobbies,
            ProfilePhotos = response.ProfilePhotos,
            UserProfileAnswers = response.UserProfileAnswers,
            DiscoveryToken = tokenService.GenerateToken(swiperId, profile.Id, profile.Mode, profile.SubscriptionPlan)
        };

        return discoveryResponse;
    }
}

