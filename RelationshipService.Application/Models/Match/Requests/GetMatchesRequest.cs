namespace RelationshipService.Application.Models.Match.Requests;

public class GetMatchesRequest
{
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 10;
}
