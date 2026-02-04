using RelationshipService.Application.Models.UserProfile.Requests;
using RelationshipService.Application.Models.UserProfile.Responses;
using RelationshipService.Application.Models.UserPrefences.Requests;

namespace RelationshipService.Application.ServiceContracts;

public interface IUserProfileService
{
    Task<UserProfileResponse> GetByUserIdAsync(Guid userId);
    Task<List<UserProfileResponse>> GetAllAsync();
    Task CreateAsync(CreateUserProfileRequest request);
    Task UpdateAsync(UpdateUserProfileRequest request);
    Task DeleteAsync(Guid userId);
    Task<List<DiscoveryProfileResponse>> GetDiscoveryProfilesAsync(Guid userId);
    Task SyncHobbiesAsync(Guid userId, SyncHobbiesRequest request);
    Task SyncAnswersAsync(Guid userId, SyncAnswersRequest request);
    Task AddPhotoAsync(Guid userId, AddPhotoRequest request);
    Task DeletePhotoAsync(Guid userId, int photoId);
    Task SetMainPhotoAsync(Guid userId, int photoId);
    Task UpdatePreferencesAsync(UpdateUserPreferencesRequest request);
}
