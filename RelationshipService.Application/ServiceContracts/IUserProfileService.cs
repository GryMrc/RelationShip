using RelationshipService.Application.Models.UserProfile.Requests;
using RelationshipService.Application.Models.UserProfile.Responses;
using RelationshipService.Application.Models.UserPrefences.Requests;

namespace RelationshipService.Application.ServiceContracts;

public interface IUserProfileService
{
    Task<UserProfileResponse> GetByUserIdAsync(int userId);
    Task<List<UserProfileResponse>> GetAllAsync();
    Task<int> CreateAsync(CreateUserProfileRequest request);
    Task UpdateAsync(UpdateUserProfileRequest request);
    Task DeleteAsync(int userId);
    Task<List<DiscoveryProfileResponse>> GetDiscoveryProfilesAsync(int userId);

    Task SyncHobbiesAsync(int userId, SyncHobbiesRequest request);
    Task SyncAnswersAsync(int userId, SyncAnswersRequest request);
    Task AddPhotoAsync(int userId, AddPhotoRequest request);
    Task DeletePhotoAsync(int userId, int photoId);
    Task SetMainPhotoAsync(int userId, int photoId);
    Task UpdatePreferencesAsync(UpdateUserPreferencesRequest request);
}
