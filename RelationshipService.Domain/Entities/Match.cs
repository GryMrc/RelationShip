using RelationshipService.Domain.Base;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Domain.Entities;

public class Match : Entity<int>
{
    public int UserAId { get; private set; }
    public int UserBId { get; private set; }
    public MatchMode MatchMode { get; set; }
    public MatchStatus MatchStatus { get; set; }
    public int FreezedUserId { get; set; }
    public int DeletedUserId { get; set; }
    public string? Reason { get; set; }
}
