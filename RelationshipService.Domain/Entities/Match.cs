using RelationshipService.Domain.Base;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Domain.Entities;

public class Match : Entity<int>
{
    public long ProfileAId { get; set; }
    public long ProfileBId { get; set; }
    public Mode Mode { get; set; }
    public MatchStatus MatchStatus { get; set; }
    public long FreezedProfileId { get; set; }
    public long DeletedProfileId { get; set; }
    public string? Reason { get; set; }
}

