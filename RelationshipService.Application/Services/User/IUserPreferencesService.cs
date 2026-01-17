using RelationshipService.Application.Models.UserPrefences.Requests;

namespace RelationshipService.Application.Services.User;

public interface IUserPreferencesService
{
    Task UpdateAsync(UpdateUserPreferencesRequest request);
}
