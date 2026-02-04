using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Events;

public enum RelationshipNotificationType
{
    NewLike = 1,
    NewMatch = 2,
    MissedMatch = 3,
    NewMessage = 4
}

public class RelationshipActionResultEvent
{
    public long UserAId { get; set; }
    public long? UserBId { get; set; }
    public RelationshipNotificationType Type { get; set; }
    public Mode Mode { get; init; }
}
