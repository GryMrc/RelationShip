namespace RelationshipService.Application.Models.Match.Responses;

public class MatchResponse
{
    public string Name { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public int MatchId { get; set; }
    public DateTime MatchDate { get; set; }
    public bool IsActive { get; set; }
}
