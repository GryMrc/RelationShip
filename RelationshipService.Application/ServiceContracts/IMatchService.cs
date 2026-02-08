using RelationshipService.Application.Models.Common;
using RelationshipService.Application.Models.Match.Responses;
using RelationshipService.Application.Models.Match.Requests;
using RelationshipService.Domain.Models;

namespace RelationshipService.Application.ServiceContracts;

public interface IMatchService
{
    Task<PaginatedResponse<MatchResponse>> GetMatchesAsync(Guid userId, GetMatchesRequest request);
    Task<UserMatchState> GetUserMatchStateAsync(Guid userId);
    Task UnmatchAsync(Guid userId, long matchId, string reason);
}
