using Microsoft.EntityFrameworkCore;
using RelationshipService.Application.Models.Common;
using RelationshipService.Application.Models.Match.Requests;
using RelationshipService.Application.Models.Match.Responses;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Services;

public class MatchService(IRelationShipDbContext context) : IMatchService
{
    public async Task<PaginatedResponse<MatchResponse>> GetMatchesAsync(int userId, GetMatchesRequest request)
    {
        // 1. Get current user's mode
        var currentUserMode = await context.UserProfiles
            .Where(u => u.Id == userId)
            .Select(u => u.Mode)
            .FirstOrDefaultAsync();

        // 2. Base Query
        var query = context.Matches
            .AsNoTracking()
            .Where(m => (m.UserAId == userId || m.UserBId == userId) && 
                         m.Mode == currentUserMode &&
                         m.MatchStatus == MatchStatus.Active);

        // 3. Count
        var totalCount = await query.CountAsync();

        // 4. Pagination
        var matches = await query
            .OrderByDescending(m => m.CreatedDate)
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var matchResponses = new List<MatchResponse>();

        // 5. Fetch User Details (Including Deleted Users via IgnoreQueryFilters)
        foreach (var match in matches)
        {
            var targetUserId = match.UserAId == userId ? match.UserBId : match.UserAId;

            // We use IgnoreQueryFilters() to get data even if the user is soft-deleted
            var userProfile = await context.UserProfiles
                .IgnoreQueryFilters() 
                .Where(u => u.Id == targetUserId)
                .Select(u => new 
                {
                    u.Id,
                    u.Name,
                    u.IsDeleted,
                    PhotoUrl = context.UserProfilePhotos
                        .Where(p => p.UserProfileId == u.Id && p.IsMain)
                        .Select(p => p.PhotoUrl)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (userProfile != null)
            {
                matchResponses.Add(new MatchResponse
                {
                    MatchId = match.Id,
                    Name = userProfile.IsDeleted ? "Deleted Account" : userProfile.Name,
                    PhotoUrl = userProfile.IsDeleted ? string.Empty : (userProfile.PhotoUrl ?? string.Empty),
                    MatchDate = match.CreatedDate,
                    IsActive = !userProfile.IsDeleted 
                });
            }
        }

        return new PaginatedResponse<MatchResponse>(matchResponses, totalCount, request.PageIndex, request.PageSize);
    }

    public async Task UnmatchAsync(int userId, int matchId, string reason)
    {
        var match = await context.Matches.FirstOrDefaultAsync
                                        (m => m.Id == matchId &&     
                                        (m.UserAId == userId || m.UserBId == userId)) 
                                        ?? throw new Exception("Match not found");

        if (match.MatchStatus == MatchStatus.Deleted)
            return;

        match.MatchStatus = MatchStatus.Deleted;
        match.DeletedUserId = userId;
        match.Reason = reason;
        
        await context.SaveChangesAsync();
    }
}
