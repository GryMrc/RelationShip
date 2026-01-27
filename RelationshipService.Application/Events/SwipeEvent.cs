using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Events;

public record SwipeEvent
{
    public int SwiperUserId { get; init; }
    public int SwipedUserId { get; init; }
    public SwipeType SwipeType { get; init; }
    public MatchMode MatchMode { get; init; }
    public bool IsMatch { get; init; }

    public DateTime CreatedAt { get; init; }
}

