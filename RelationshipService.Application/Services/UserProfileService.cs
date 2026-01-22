using Microsoft.EntityFrameworkCore;
using RelationshipService.Application.Models.UserProfile.Requests;
using RelationshipService.Application.Models.UserProfile.Responses;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Domain.Entities;
using NetTopologySuite.Geometries;
using RelationshipService.Domain.Enums;
using RelationshipService.Application.Models.Hobby.Responses;

namespace RelationshipService.Application.Services;

public class UserProfileService(IRelationShipDbContext context) : IUserProfileService
{
    public async Task<UserProfileResponse> GetByUserIdAsync(int userId)
    {
        var profile = await context.UserProfiles
            .AsNoTracking()
            .Include(x => x.Preferences)
            .Include(x => x.Hobbies)
            .Include(x => x.ProfilePhotos)
            .Include(x => x.UserProfileAnswers)
                .ThenInclude(x => x.QuestionAnswer)
                    .ThenInclude(x => x.Question)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile == null) return null;

        return MapToResponse(profile);
    }

    public async Task<List<UserProfileResponse>> GetAllAsync()
    {
        var profiles = await context.UserProfiles
            .AsNoTracking()
            .Include(x => x.Preferences)
            .Include(x => x.Hobbies)
            .Include(x => x.ProfilePhotos)
            .Include(x => x.UserProfileAnswers)
                .ThenInclude(x => x.QuestionAnswer)
                    .ThenInclude(x => x.Question)
            .ToListAsync();

        return profiles.Select(MapToResponse).ToList();
    }

    public async Task<int> CreateAsync(CreateUserProfileRequest request)
    {
        var user = await context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == request.UserId);

        if (user != null) throw new Exception("Profile already exists");

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

    public async Task DeleteAsync(int userId)
    {
        var profile = await context.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);

        if (profile == null) return;

        context.UserProfiles.Remove(profile);
        await context.SaveChangesAsync();
    }

    public async Task<List<UserProfileResponse>> GetDiscoveryProfilesAsync(int userId)
    {
        // 1. Mevcut kullanıcının profilini ve tercihlerini al
        var currentUser = await context.UserProfiles
            .AsNoTracking()
            .Include(x => x.Preferences)
            .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);

        if (currentUser == null || currentUser.Preferences == null)
            return new List<UserProfileResponse>();

        // 2. Daha önce etkileşim kurduğu (swipe) kullanıcıları al (Bunları keşiften elliyoruz)
        var swipedUserIds = await context.Swipes
            .Where(s => s.SwiperUserId == userId)
            .Select(s => s.SwipedUserId)
            .ToListAsync();

        // 3. Yaş filtresi için tarih sınırlarını belirle
        var today = DateTime.UtcNow;
        var minBirthDate = today.AddYears(-currentUser.Preferences.MaxAgePreference);
        var maxBirthDate = today.AddYears(-currentUser.Preferences.MinAgePreference);

        // 4. Mesafe filtresi (KM -> Derece yaklaşık dönüşümü)
        // 1 derece yaklaşık 111.1km'dir. Derece üzerinden sorgu atmak GIST indeksini verimli kullandırır.
        double distanceLimitDegrees = currentUser.Preferences.MaxDistancePreference / 111.1;

        // 5. Ana Keşif Sorgusu (Discovery Query)
        var profiles = await context.UserProfiles
            .AsNoTracking()
            .Where(p => p.UserId != userId && !p.IsDeleted)
            .Where(p => !swipedUserIds.Contains(p.UserId)) // Daha önce swipe edilmemişler
            .Where(p => p.Gender == currentUser.Preferences.InterestedInGender) // Cinsiyet tercihi
            .Where(p => p.DateOfBirth >= minBirthDate && p.DateOfBirth <= maxBirthDate) // Yaş tercihi
            .Where(p => p.Location.Distance(currentUser.Location) <= distanceLimitDegrees) // Mesafe tercihi
            .OrderBy(x => EF.Functions.Random()) // Rastgelelik ekleyerek her defasında farklı profiller getiriyoruz
            .Take(20)
            .ToListAsync();

        return profiles.Select(MapToResponse).ToList();
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

    public async Task SyncHobbiesAsync(int userId, SyncHobbiesRequest request)
    {
        var profile = await context.UserProfiles
            .Include(x => x.Hobbies)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile == null) throw new Exception("Profile not found");

        // 1. Get requested hobbies from DB
        var hobbies = await context.Hobbies
            .Where(h => request.HobbyIds.Contains(h.Id))
            .ToListAsync();

        // 2. Clear and set (Sync)
        profile.Hobbies.Clear();
        foreach (var hobby in hobbies)
        {
            profile.Hobbies.Add(hobby);
        }

        await context.SaveChangesAsync();
    }

    public async Task SyncAnswersAsync(int userId, SyncAnswersRequest request)
    {
        var profile = await context.UserProfiles
            .Include(x => x.UserProfileAnswers)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile == null) throw new Exception("Profile not found");

        // Simple sync: Remove all and add new ones (or diff if complex logic needed)
        context.UserProfileAnswers.RemoveRange(profile.UserProfileAnswers);

        profile.UserProfileAnswers = request.Answers.Select(a => new UserProfileAnswer
        {
            UserProfileId = profile.Id,
            QuestionAnswerId = a.QuestionAnswerId
        }).ToList();

        await context.SaveChangesAsync();
    }

    public async Task AddPhotoAsync(int userId, AddPhotoRequest request)
    {
        var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        if (profile == null) throw new Exception("Profile not found");

        var photo = new UserProfilePhoto
        {
            UserProfileId = profile.Id,
            PhotoUrl = request.PhotoUrl,
            IsMain = request.IsMain,
            Order = request.Order
        };

        context.UserProfilePhotos.Add(photo);
        await context.SaveChangesAsync();
    }

    public async Task DeletePhotoAsync(int userId, int photoId)
    {
        var photo = await context.UserProfilePhotos
            .FirstOrDefaultAsync(x => x.Id == photoId && x.UserProfile.UserId == userId);

        if (photo == null) return;

        context.UserProfilePhotos.Remove(photo);
        await context.SaveChangesAsync();
    }

    public async Task SetMainPhotoAsync(int userId, int photoId)
    {
        var photos = await context.UserProfilePhotos
            .Where(x => x.UserProfile.UserId == userId)
            .ToListAsync();

        foreach (var p in photos)
        {
            p.IsMain = (p.Id == photoId);
        }

        await context.SaveChangesAsync();
    }
}
