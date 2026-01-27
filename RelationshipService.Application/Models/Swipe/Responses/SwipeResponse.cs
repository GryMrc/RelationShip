using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Models.Swipe.Responses;

public class SwipeResponse
{
    public bool IsMatch { get; set; }
    public SwipeType? SwipeType { get; set; }
    public SwipeType? MatchedSwipeType { get; set; }
}



