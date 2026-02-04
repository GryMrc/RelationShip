using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Events;

public record SwipeEvent
{
    public Guid SwiperId { get; init; }
    public long SwipedProfilId { get; init; }
    public SwipeType SwipeType { get; init; }
    public Mode Mode { get; init; }
    public bool IsRedisMatch { get; init; }
    public int? OppositeSwipeType { get; init; }
}

