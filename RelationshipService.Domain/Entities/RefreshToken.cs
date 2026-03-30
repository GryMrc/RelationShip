using RelationshipService.Domain.Base;

namespace RelationshipService.Domain.Entities
{
    public class RefreshToken : Entity<long>
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public string? ReplacedByToken { get; set; }
        
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;

        public virtual User User { get; set; } = null!;
    }
}
