namespace RelationshipService.Domain.Models
{
    public class UserMatchState
    {
        public long ProfileId { get; set; }
        public List<MatchSyncItem> Matches { get; set; } = new();
    }

    public class MatchSyncItem
    {
        public long MatchedProfileId { get; set; }
        public long MatchId { get; set; }
    }
}
