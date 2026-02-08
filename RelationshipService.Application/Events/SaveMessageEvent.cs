namespace RelationshipService.Application.Events
{
    public record SaveMessageEvent
    {
        public long MatchId { get; init; }
        public long SenderProfileId { get; init; }
        public long ReceiverProfileId { get; init; }
        public string Content { get; init; }
    }
}
