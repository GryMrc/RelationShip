namespace RelationshipService.Domain.Entities;

public class UserLocationHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public DateTime RecordedAt { get; set; }
}
