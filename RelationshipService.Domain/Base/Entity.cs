namespace RelationshipService.Domain.Base;

public class Entity<T> : IEntity<T>
{
    public T Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsDeleted { get; set; }
}
