using Microsoft.EntityFrameworkCore;
using RelationshipService.Application.Models.UserProfile.Requests;
using RelationshipService.Application.Models.UserProfile.Responses;
using RelationshipService.Application.Models.UserPrefences.Requests;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Domain.Entities;
using NetTopologySuite.Geometries;
using RelationshipService.Application.Mappers;

namespace RelationshipService.Application.Services;

public class UserProfileService(
    IRelationShipDbContext context, 
    IDiscoveryTokenService tokenService) : IUserProfileService
{

    public async Task<UserProfileResponse?> GetByUserIdAsync(Guid userId)
    {
        var profile = await context.UserProfiles
            .AsNoTracking()
            .Include(x => x.Preferences)
            .Include(x => x.Hobbies)
            .Include(x => x.Photos)
            .Include(x => x.Answers)
                .ThenInclude(x => x.QuestionAnswer)
                    .ThenInclude(x => x.Question)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        return profile?.ToResponse();
    }

    public async Task<List<UserProfileResponse>> GetAllAsync()
    {
        var profiles = await context.UserProfiles
            .AsNoTracking()
            .Include(x => x.Preferences)
            .Include(x => x.Hobbies)
            .Include(x => x.Photos)
            .Include(x => x.Answers)
                .ThenInclude(x => x.QuestionAnswer)
                    .ThenInclude(x => x.Question)
            .ToListAsync();

        return profiles.Select(p => p.ToResponse()).ToList();
    }

    public async Task CreateAsync(CreateUserProfileRequest request)
    {
        var user = await context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == request.UserId);

        if (user != null) throw new Exception("Profile already exists");

        var profile = new Profile
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
    }

    public async Task UpdateAsync(UpdateUserProfileRequest request)
    {
        var profile = await context.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == request.UserId);

        if (profile == null) throw new Exception("Profile not found");

        profile.Bio = request.Bio;
        profile.Height = request.Height;
        profile.Weight = request.Weight;
        profile.ZodiacSign = request.ZodiacSign;
        profile.RisingZodiacSign = request.RisingZodiacSign;
        profile.Location = new Point(request.Longitude, request.Latitude) { SRID = 4326 };

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid userId)
    {
        var profile = await context.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);

        if (profile == null) return;

        context.UserProfiles.Remove(profile);
        await context.SaveChangesAsync();
    }

    public async Task<List<DiscoveryProfileResponse>> GetDiscoveryProfilesAsync(Guid userId)
    {
        var currentUser = await context.UserProfiles
            .AsNoTracking()
            .Include(x => x.Preferences)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (currentUser == null || currentUser.Preferences == null)
            return new List<DiscoveryProfileResponse>();

        var swipedUserIds = await context.Swipes
            .Where(s => s.SwiperProfileId == currentUser.Id)
            .Select(s => s.SwipedProfileId)
            .ToListAsync();

        var today = DateTime.UtcNow;
        var minBirthDate = today.AddYears(-currentUser.Preferences.MaxAgePreference);
        var maxBirthDate = today.AddYears(-currentUser.Preferences.MinAgePreference);
        double distanceLimitDegrees = currentUser.Preferences.MaxDistancePreference / 111.1;

        var profiles = await context.UserProfiles
            .AsNoTracking()
            .Include(x => x.Preferences)
            .Include(x => x.Photos)
            .Include(x => x.Hobbies)
            .Include(x => x.Answers)
                .ThenInclude(x => x.QuestionAnswer)
                    .ThenInclude(x => x.Question)
            .Where(p => p.UserId != userId && !p.IsDeleted)
            .Where(p => !swipedUserIds.Contains(p.Id))
            .Where(p => p.Gender == currentUser.Preferences.InterestedInGender)
            .Where(p => p.Mode == currentUser.Mode) // Same world filter (Optimized: No JOIN)
            .Where(p => p.DateOfBirth >= minBirthDate && p.DateOfBirth <= maxBirthDate)
            .Where(p => p.Location.Distance(currentUser.Location) <= distanceLimitDegrees)
            .OrderBy(x => EF.Functions.Random())
            .Take(20)
            .ToListAsync();

        return profiles.Select(p => p.ToDiscoveryResponse(userId, tokenService)).ToList();
    }



    public async Task SyncHobbiesAsync(Guid userId, SyncHobbiesRequest request)
    {
        var profile = await context.UserProfiles
            .Include(x => x.Hobbies)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile == null) throw new Exception("Profile not found");

        var hobbies = await context.Hobbies
            .Where(h => request.HobbyIds.Contains(h.Id))
            .ToListAsync();

        profile.Hobbies.Clear();
        foreach (var hobby in hobbies)
        {
            profile.Hobbies.Add(hobby);
        }

        await context.SaveChangesAsync();
    }

    public async Task SyncAnswersAsync(Guid userId, SyncAnswersRequest request)
    {
        var profile = await context.UserProfiles
            .Include(x => x.Answers)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile == null) throw new Exception("Profile not found");

        context.UserProfileAnswers.RemoveRange(profile.Answers);

        profile.Answers = request.Answers.Select(a => new ProfileAnswer
        {
            ProfileId = profile.Id,
            QuestionAnswerId = a.QuestionAnswerId
        }).ToList();

        await context.SaveChangesAsync();
    }

    public async Task AddPhotoAsync(Guid userId, AddPhotoRequest request)
    {
        var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        if (profile == null) throw new Exception("Profile not found");

        var photo = new ProfilePhoto
        {
            ProfileId = profile.Id,
            PhotoUrl = request.PhotoUrl,
            IsMain = request.IsMain,
            Order = request.Order
        };

        context.UserProfilePhotos.Add(photo);
        await context.SaveChangesAsync();
    }

    public async Task DeletePhotoAsync(Guid userId, int photoId)
    {
        var photo = await context.UserProfilePhotos
            .FirstOrDefaultAsync(x => x.Id == photoId && x.Profile.UserId == userId);

        if (photo == null) return;

        context.UserProfilePhotos.Remove(photo);
        await context.SaveChangesAsync();
    }

    public async Task SetMainPhotoAsync(Guid userId, int photoId)
    {
        var photos = await context.UserProfilePhotos
            .Where(x => x.Profile.UserId == userId)
            .ToListAsync();

        foreach (var p in photos)
        {
            p.IsMain = (p.Id == photoId);
        }

        await context.SaveChangesAsync();
    }

    public async Task UpdatePreferencesAsync(UpdateUserPreferencesRequest request)
    {
        var profile = await context.UserProfiles
            .Include(x => x.Preferences)
            .FirstOrDefaultAsync(x => x.UserId == request.UserId);

        if (profile == null) throw new Exception("Profile not found");

        if (profile.Preferences == null)
        {
            profile.Preferences = new ProfilePreferences
            {
                ProfileId = profile.Id,
                InterestedInGender = request.InterestedInGender,
                MaxDistancePreference = request.MaxDistancePreference,
                MinAgePreference = request.MinAgePreference,
                MaxAgePreference = request.MaxAgePreference
            };
        }
        else
        {
            profile.Preferences.InterestedInGender = request.InterestedInGender;
            profile.Preferences.MaxDistancePreference = request.MaxDistancePreference;
            profile.Preferences.MinAgePreference = request.MinAgePreference;
            profile.Preferences.MaxAgePreference = request.MaxAgePreference;
        }

        profile.Mode = request.Mode;
        await context.SaveChangesAsync();
    }
}



