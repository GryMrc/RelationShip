using System;
using System.Collections.Generic;

namespace RelationshipService.Domain.Models
{
    public class SocketClient
    {
        public string ConnectionId { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public long ProfileId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
        public List<MatchSyncItem> Matches { get; set; } = new();
    }
}
