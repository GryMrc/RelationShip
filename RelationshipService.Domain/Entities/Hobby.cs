using RelationshipService.Domain.Base;

namespace RelationshipService.Domain.Entities;

public class Hobby : Entity<int>
{
    public string Name { get; set; }
}