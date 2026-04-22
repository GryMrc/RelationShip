using RelationshipService.Domain.Base;

namespace RelationshipService.Domain.Entities;

public class ProfileDevice : Entity<long>
{
    public long ProfileId { get; set; }
    public string Token { get; set; }
    public string Platform { get; set; }
    public Guid DeviceId { get; set; }
    public string Name { get; set; }

    public Profile Profile { get; set; }
}
