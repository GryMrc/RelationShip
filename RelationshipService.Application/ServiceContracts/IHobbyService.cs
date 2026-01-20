using RelationshipService.Application.Models.Hobby.Requests;
using RelationshipService.Application.Models.Hobby.Responses;

namespace RelationshipService.Application.ServiceContracts;

public interface IHobbyService
{
    Task<IEnumerable<HobbyResponse>> GetAllAsync();
    Task<HobbyResponse?> GetByIdAsync(int id);
    Task<HobbyResponse> CreateAsync(CreateHobbyRequest request);
    Task<HobbyResponse?> UpdateAsync(UpdateHobbyRequest request);
    Task<bool> DeleteAsync(int id);
}
