using Microsoft.EntityFrameworkCore;
using RelationshipService.Application.Models.UserPrefences.Requests;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Application.Services;

public class UserPreferencesService(IRelationShipDbContext context) : IUserPreferencesService
{
    public async Task UpdateAsync(UpdateUserPreferencesRequest request)
    {
        var preferences = await context.UserPreferences
            .FirstOrDefaultAsync(x => x.UserProfile.UserId == request.UserId);

        if (preferences == null)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == request.UserId);
            if (profile == null) throw new Exception("Profile not found");

            preferences = new UserPreferences
            {
                UserProfileId = profile.Id,
                InterestedInGender = request.InterestedInGender,
                MaxDistancePreference = request.MaxDistancePreference,
                MinAgePreference = request.MinAgePreference,
                MaxAgePreference = request.MaxAgePreference
            };
            context.UserPreferences.Add(preferences);
        }
        else
        {
            preferences.InterestedInGender = request.InterestedInGender;
            preferences.MaxDistancePreference = request.MaxDistancePreference;
            preferences.MinAgePreference = request.MinAgePreference;
            preferences.MaxAgePreference = request.MaxAgePreference;
        }

        await context.SaveChangesAsync();
    }
}
