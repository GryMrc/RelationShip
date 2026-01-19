using RelationshipService.Application.Models.UserPrefences.Requests;

namespace RelationshipService.Application.ServiceContracts;

public interface IUserPreferencesService
{
    Task UpdateAsync(UpdateUserPreferencesRequest request);
}
