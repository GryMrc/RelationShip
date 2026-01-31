using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Events;

public enum RelationshipNotificationType
{
    NewLike = 1,
    NewMatch = 2,
    NewMessage = 3
}

public class RelationshipActionResultEvent
{
    public int UserAId { get; set; }
    public int? UserBId { get; set; }
    public RelationshipNotificationType Type { get; set; }
    public MatchMode MatchMode { get; set; }
}
