using RelationshipService.Domain.Base;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Domain.Entities;

public class Match : Entity<int>
{
    private Match() { } // For EF Core

    public Match(int userAId, int userBId, Mode mode = Mode.Date)
    {
        UserAId = userAId;
        UserBId = userBId;
        Mode = mode;
        MatchStatus = MatchStatus.Active;
    }

    public int UserAId { get; private set; }
    public int UserBId { get; private set; }
    public Mode Mode { get; set; }
    public MatchStatus MatchStatus { get; set; }
    public int FreezedUserId { get; set; }
    public int DeletedUserId { get; set; }
    public string? Reason { get; set; }
}

