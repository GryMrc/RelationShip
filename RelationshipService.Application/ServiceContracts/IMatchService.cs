using RelationshipService.Application.Models.Common;
using RelationshipService.Application.Models.Match.Responses;
using RelationshipService.Application.Models.Match.Requests;

namespace RelationshipService.Application.ServiceContracts;

public interface IMatchService
{
    Task<PaginatedResponse<MatchResponse>> GetMatchesAsync(Guid userId, GetMatchesRequest request);
    Task UnmatchAsync(Guid userId, int matchId, string reason);
}
