using RelationshipService.Application.Models.Swipe.Requests;
using RelationshipService.Application.Models.Swipe.Responses;
using RelationshipService.Application.Models.UserProfile.Responses;

namespace RelationshipService.Application.ServiceContracts;

public interface ISwipeService
{
    Task<SwipeResponse> SwipeAsync(Guid userId, SwipeRequest request);
    Task<List<UserProfileResponse>> GetLikersAsync(Guid userId);
}
