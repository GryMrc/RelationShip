namespace RelationshipService.Domain.Entities;

public class Match
{
    public int Id { get; private set; }
    public int UserAId { get; private set; }
    public int UserBId { get; private set; }
    public DateTime MatchedAt { get; private set; }
    public MatchMode MatchMode { get; set; }
    public int FreezedUserId { get; set; }
    public int DeletedUserId { get; set; }
}
