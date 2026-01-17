using Microsoft.EntityFrameworkCore;
using RelationshipService.Application;
using RelationshipService.Application.Models.UserLocation.Requests;
using RelationshipService.Application.Services.User;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Infra.Services.User;

public class UserLocationService(IRelationShipDbContext context) : IUserLocationService
{
    public async Task UpdateAsync(UpdateUserLocationRequest request)
    {
        var location = await context.UserLocations
            .FirstOrDefaultAsync(x => x.UserProfile.UserId == request.UserId);

        if (location == null)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == request.UserId);
            if (profile == null) throw new Exception("Profile not found");

            location = new UserLocation
            {
                UserProfileId = profile.Id,
                Latitude = request.Latitude,
                Longitude = request.Longitude
            };
            context.UserLocations.Add(location);
        }
        else
        {
            location.Latitude = request.Latitude;
            location.Longitude = request.Longitude;
        }

        // Also log to history
        context.UserLocationHistories.Add(new UserLocationHistory
        {
            UserId = request.UserId,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        });

        await context.SaveChangesAsync();
    }
}
