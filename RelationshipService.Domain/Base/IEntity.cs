namespace RelationshipService.Domain.Base;

public interface IEntity<T> : IEntity
{
    public T Id { get; set; }
}
public interface IEntity
{
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsDeleted { get; set; }
}
