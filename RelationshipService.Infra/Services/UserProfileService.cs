using Microsoft.EntityFrameworkCore;
using RelationshipService.Application;
using RelationshipService.Application.Models.UserProfile.Requests;
using RelationshipService.Application.Models.UserProfile.Responses;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Domain.Entities;
using NetTopologySuite.Geometries;

namespace RelationshipService.Infra.Services;

public class UserProfileService(IRelationShipDbContext context) : IUserProfileService
{
    public async Task<UserProfileResponse> GetByUserIdAsync(int userId)
    {
        var profile = await context.UserProfiles
            .Include(x => x.Preferences)
            .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);

        if (profile == null) return null;

        return MapToResponse(profile);
    }

    public async Task<List<UserProfileResponse>> GetAllAsync()
    {
        var profiles = await context.UserProfiles
            .Include(x => x.Preferences)
            .Where(x => !x.IsDeleted)
            .ToListAsync();

        return profiles.Select(MapToResponse).ToList();
    }

    public async Task<int> CreateAsync(CreateUserProfileRequest request)
    {
        var profile = new UserProfile
        {
            UserId = request.UserId,
            Name = request.Name,
            Bio = request.Bio,
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth,
            Height = request.Height,
            Weight = request.Weight,
            ZodiacSign = request.ZodiacSign,
            RisingZodiacSign = request.RisingZodiacSign,
            Location = new Point(request.Longitude, request.Latitude) { SRID = 4326 }
        };

        context.UserProfiles.Add(profile);
        await context.SaveChangesAsync();

        return profile.Id;
    }

    public async Task UpdateAsync(UpdateUserProfileRequest request)
    {
        var profile = await context.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsDeleted);

        if (profile == null) throw new Exception("Profile not found");

        profile.Name = request.Name;
        profile.Bio = request.Bio;
        profile.Gender = request.Gender;
        profile.DateOfBirth = request.DateOfBirth;
        profile.Height = request.Height;
        profile.Weight = request.Weight;
        profile.ZodiacSign = request.ZodiacSign;
        profile.RisingZodiacSign = request.RisingZodiacSign;
        profile.Location = new Point(request.Longitude, request.Latitude) { SRID = 4326 };

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int userId)
    {
        var profile = await context.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);

        if (profile == null) return;

        context.UserProfiles.Remove(profile);
        await context.SaveChangesAsync();
    }

    private static UserProfileResponse MapToResponse(UserProfile profile)
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
            InterestedInGender = profile.Preferences?.InterestedInGender ?? default,
            MaxDistancePreference = profile.Preferences?.MaxDistancePreference ?? 0,
            MinAgePreference = profile.Preferences?.MinAgePreference ?? 0,
            MaxAgePreference = profile.Preferences?.MaxAgePreference ?? 0
        };
    }
}
