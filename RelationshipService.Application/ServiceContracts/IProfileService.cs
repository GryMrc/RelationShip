using RelationshipService.Application.Models.Profile.Requests;
using RelationshipService.Application.Models.Profile.Responses;

namespace RelationshipService.Application.ServiceContracts;

public interface IProfileService
{
    Task<ProfileResponse> GetByUserIdAsync(Guid userId);
    Task<List<ProfileResponse>> GetAllAsync();
    Task CreateAsync(CreateProfileRequest request);
    Task UpdateAsync(UpdateProfileRequest request);
    Task DeleteAsync(Guid userId);
    Task<List<DiscoveryProfileResponse>> GetDiscoveryProfilesAsync(Guid userId);
    Task SyncHobbiesAsync(Guid userId, SyncHobbiesRequest request);
    Task SyncAnswersAsync(Guid userId, SyncAnswersRequest request);
    Task AddPhotoAsync(Guid userId, AddProfilePhotoRequest request);
    Task DeletePhotoAsync(Guid userId, int photoId);
    Task SetMainPhotoAsync(Guid userId, int photoId);
    Task UpdatePreferencesAsync(UpdateProfilePreferencesRequest request);
    Task SyncDeviceAsync(Guid userId, UpsertProfileDeviceRequest request);
}
