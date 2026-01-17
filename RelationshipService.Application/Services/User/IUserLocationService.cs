using RelationshipService.Application.Models.UserLocation.Requests;

namespace RelationshipService.Application.Services.User;

public interface IUserLocationService
{
    Task UpdateAsync(UpdateUserLocationRequest request);
}
