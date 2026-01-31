using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Events;

public record SwipeEvent
{
    public int SwiperUserId { get; init; }
    public int SwipedUserId { get; init; }
    public SwipeType SwipeType { get; init; }
    public Mode Mode { get; init; }
    public bool IsRedisMatch { get; init; }
    public int? OppositeSwipeType { get; init; }
}

