using RelationshipService.Application.Models.Swipe.Requests;
using RelationshipService.Application.Models.Swipe.Responses;
using RelationshipService.Application.Models.UserProfile.Responses;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.ServiceContracts;

public interface ISwipeService
{
    Task<SwipeResponse> SwipeAsync(int swiperId, SwipeRequest request);
    Task<List<UserProfileResponse>> GetLikersAsync(int userId);
}
