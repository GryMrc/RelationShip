using RelationshipService.Domain.Base;

namespace RelationshipService.Domain.Entities
{
    public class Message : Entity<long>
    {
        public long MatchId { get; set; }
        public long SenderProfileId { get; set; }
        public long ReceiverProfileId { get; set; }
        public string Content { get; set; }

        public virtual Match Match { get; set; }
        public virtual Profile SenderProfile { get; set; }
        public virtual Profile ReceiverProfile { get; set; }
    }
}
