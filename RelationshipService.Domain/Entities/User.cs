using RelationshipService.Domain.Base;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Domain.Entities
{
    public class User : Entity<Guid>
    {
        public string Email { get; set; } = null!;
        public string Provider { get; set; } = null!; // Google, Apple
        public string ProviderKey { get; set; } = null!; // Social Subject ID
        public Roles Role { get; set; } = Roles.User;
        
        // Navigation property for Profile
        public virtual Profile? Profile { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
