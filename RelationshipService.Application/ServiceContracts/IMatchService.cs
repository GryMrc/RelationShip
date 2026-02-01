using RelationshipService.Application.Models.Common;
using RelationshipService.Application.Models.Match.Responses;
using RelationshipService.Application.Models.Match.Requests;

namespace RelationshipService.Application.ServiceContracts;

public interface IMatchService
{
    Task<PaginatedResponse<MatchResponse>> GetMatchesAsync(int userId, GetMatchesRequest request);
    Task UnmatchAsync(int userId, int matchId, string reason);
}
