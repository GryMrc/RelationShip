using MassTransit;
using Microsoft.EntityFrameworkCore;
using RelationshipService.Application.Models.Common;
using RelationshipService.Application.Models.Match.Requests;
using RelationshipService.Application.Models.Match.Responses;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Domain.Enums;
using RelationshipService.Domain.Models;

namespace RelationshipService.Application.Services;

public class MatchService(IRelationShipDbContext context, IPublishEndpoint publishEndpoint) : IMatchService
{
    public async Task<PaginatedResponse<MatchResponse>> GetMatchesAsync(Guid userId, GetMatchesRequest request)
    {
        // 1. Get current user's mode
        var currentProfile = await context.UserProfiles
            .Where(u => u.UserId == userId)
            .Select(u => new
            {
                u.Id,
                u.Mode
            }).FirstOrDefaultAsync();

        if (currentProfile == null) 
        {
            throw new Exception("User not found");
        }

        // 2. Base Query
        var query = context.Matches
            .AsNoTracking()
            .Where(m => (m.ProfileAId == currentProfile.Id || m.ProfileBId == currentProfile.Id) && 
                         m.Mode == currentProfile.Mode &&
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
            var targetProfilId = match.ProfileAId == currentProfile.Id ? match.ProfileBId : match.ProfileAId;

            // We use IgnoreQueryFilters() to get data even if the user is soft-deleted
            var userProfile = await context.UserProfiles
                .IgnoreQueryFilters() 
                .Where(u => u.Id == targetProfilId)
                .Select(u => new 
                {
                    u.Id,
                    u.Name,
                    u.IsDeleted,
                    PhotoUrl = context.UserProfilePhotos
                        .Where(p => p.ProfileId == u.Id && p.IsMain)
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

    public async Task<UserMatchState> GetUserMatchStateAsync(Guid userId)
    {
        var currentProfile = await context.UserProfiles
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .Select(u => new { u.Id, u.Mode })
            .FirstOrDefaultAsync() ?? throw new Exception("User not found");

        var matches = await context.Matches
            .AsNoTracking()
            .Where(m => (m.ProfileAId == currentProfile.Id || m.ProfileBId == currentProfile.Id) &&
                         m.Mode == currentProfile.Mode &&
                         m.MatchStatus == MatchStatus.Active)
            .Select(m => new MatchSyncItem
            { 
                MatchedProfileId = m.ProfileAId == currentProfile.Id ? m.ProfileBId : m.ProfileAId,
                MatchId = m.Id 
            })
            .ToListAsync();

        return new UserMatchState
        {
            ProfileId = currentProfile.Id,
            Matches = matches
        };
    }

    public async Task UnmatchAsync(Guid userId, long matchId, string reason)
    {
        var currentProfile = await context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId)
            ?? throw new Exception("User not found");

        var match = await context.Matches.FirstOrDefaultAsync
                                        (m => m.Id == matchId &&     
                                        (m.ProfileAId == currentProfile.Id || m.ProfileBId == currentProfile.Id)) 
                                        ?? throw new Exception("Match not found");

        if (match.MatchStatus == MatchStatus.Deleted)
            return;

        var otherProfileId = match.ProfileAId == currentProfile.Id ? match.ProfileBId : match.ProfileAId;

        match.MatchStatus = MatchStatus.Deleted;
        match.DeletedProfileId = currentProfile.Id;
        match.Reason = reason;
        
        await context.SaveChangesAsync();

        // Publish event to notify WebSocket server
        await publishEndpoint.Publish(new RelationshipService.Application.Events.RelationshipActionResultEvent
        {
            UserAId = currentProfile.Id,
            UserBId = otherProfileId,
            MatchId = matchId,
            Type = RelationshipService.Application.Events.RelationshipNotificationType.Unmatch,
            Mode = match.Mode
        });
    }
}
