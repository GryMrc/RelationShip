using RelationshipService.Application.Models.Swipe.Requests;
using RelationshipService.Application.Models.Swipe.Responses;

namespace RelationshipService.Application.ServiceContracts;

public interface ISwipeService
{
    Task<SwipeResponse> SwipeAsync(int swiperId, SwipeRequest request);
}
