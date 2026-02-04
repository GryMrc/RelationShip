using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RelationshipService.Application.Events;
using RelationshipService.Application.Models.Swipe.Requests;
using RelationshipService.Application.Models.Swipe.Responses;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Domain.Enums;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using RelationshipService.Application.Mappers;
using RelationshipService.Application.Models.UserProfile.Responses;
using RelationshipService.Application.Models.Hobby.Responses;

namespace RelationshipService.Application.Services;

public class SwipeService(
    IDiscoveryTokenService tokenService,
    IConnectionMultiplexer redis,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration,
    IRelationShipDbContext context,
    ILogger<SwipeService> logger) : ISwipeService
{
    private readonly IDatabase _db = redis.GetDatabase();

    private enum RedisSwipeStatus
    {
        Success = 0,
        LimitReached = 1,
        AlreadySwiped = 2
    }

    private const string SwipeLuaScript = @"
        local limit = tonumber(ARGV[1])
        local limitKey = KEYS[1]
        local swipeKey = KEYS[2]
        local targetSwipeKey = KEYS[3]
        local targetId = ARGV[2]
        local swipeType = tonumber(ARGV[3])
        local swiperId = ARGV[4]
        local expirySeconds = 172800 -- 48h

        -- 1. Daily Limit Check
        if limit ~= -1 then
            local current = redis.call('GET', limitKey)
            if current and tonumber(current) >= limit then return {1, 0, 0, 0} end
            redis.call('INCR', limitKey)
            if not current then redis.call('EXPIRE', limitKey, 86400) end
        end

        -- 2. Duplicate Check & Save
        local wasSet = redis.call('HSETNX', swipeKey, targetId, swipeType)
        if wasSet == 0 then return {2, 0, 0, 0} end
        redis.call('EXPIRE', swipeKey, expirySeconds)

        -- 3. Match Check
        local isMatch = 0
        local matchedType = -1
        local targetFound = 0
        local targetVal = redis.call('HGET', targetSwipeKey, swiperId)
        
        if targetVal then
            targetFound = 1
            matchedType = tonumber(targetVal)
            -- Match only if both are NOT Dislike (0)
            if swipeType ~= 0 and matchedType ~= 0 then isMatch = 1 end
        end
        return {0, isMatch, matchedType, targetFound}";

    public async Task<SwipeResponse> SwipeAsync(Guid swiperId, SwipeRequest request)
    {
        // 1. Validate Token (Quick cryptographic check)
        if (!tokenService.ValidateToken(swiperId, request.TargetUserId, request.Mode, request.DiscoveryToken, out var plan))
        {
            throw new Exception("Invalid or expired discovery token for this mode");
        }

        int? oppositeTypeFromRedis = null;
        bool isMatch = false;

        try
        {
            var limit = configuration.GetValue<int>($"SwipeSettings:DailyLimits:{plan}");
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var res = await ExecuteSwipeScript(limit, today, swiperId, request);

            switch (res.Status)
            {
                case RedisSwipeStatus.LimitReached:
                    throw new Exception($"You have reached your daily swipe limit for {plan} plan.");
                case RedisSwipeStatus.AlreadySwiped:
                    throw new Exception("You have already swiped on this user in this mode");
            }

            if (res.TargetFound)
            {
                oppositeTypeFromRedis = res.OppositeSwipeType;
                isMatch = res.IsMatch;
            }
        }
        catch (RedisException ex)
        {
            logger.LogError(ex, "Redis Error during SwipeAsync for user {SwiperId}. Processing in background.", swiperId);
        }

        // 5. Fire and Forget to Consumer
        await publishEndpoint.Publish(new SwipeEvent
        {
            SwiperId = swiperId,
            SwipedProfilId = request.TargetUserId,
            SwipeType = request.SwipeType,
            Mode = request.Mode,
            IsRedisMatch = isMatch,
            OppositeSwipeType = oppositeTypeFromRedis
        }, context => 
        {
            // Sharding: Set routing key to isolation by mode at RabbitMQ level
            context.SetRoutingKey(request.Mode.ToString().ToLower());
        });

        return new SwipeResponse 
        { 
            IsMatch = isMatch,
            SwipeType = isMatch ? request.SwipeType : null,
            MatchedSwipeType = isMatch ? (SwipeType?)oppositeTypeFromRedis : null
        };
    }

    public async Task<List<UserProfileResponse>> GetLikersAsync(Guid userId)
    {
        // 0. Get User's current Mode and Plan
        var userProps = await context.UserProfiles
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .Select(u => new { u.Mode, u.SubscriptionPlan, u.Id })
            .FirstOrDefaultAsync()
            ?? throw new Exception("User Not Found");

        if (userProps == null) return new List<UserProfileResponse>();

        // 1. Get IDs of users who liked me in this mode
        var likerIds = await context.Swipes
            .AsNoTracking()
            .Where(s => s.SwipedProfilId == userProps.Id 
                     && s.SwipeType != SwipeType.Dislike 
                     && s.Mode == userProps.Mode)
            .Select(s => s.SwiperProfilId)
            .Distinct()
            .ToListAsync();

        if (!likerIds.Any())
            return new List<UserProfileResponse>();

        // 2. exclude matches (or any interaction from me to them)
        // If I also swiped them (Like or Dislike), I shouldn't see them in "Liked Me"
        var myInteractions = await context.Swipes
            .AsNoTracking()
            .Where(s => s.SwiperProfilId == userProps.Id 
                     && likerIds.Contains(s.SwipedProfilId) 
                     && s.Mode == userProps.Mode)
            .Select(s => s.SwipedProfilId)
            .ToListAsync();

        var pendingIds = likerIds.Except(myInteractions).ToList();

        if (!pendingIds.Any())
            return new List<UserProfileResponse>();

        // 3. Load profiles
        var profiles = await context.UserProfiles
            .AsNoTracking()
            .Include(p => p.ProfilePhotos)
            .Include(p => p.Hobbies)
            .Include(p => p.UserProfileAnswers)
                .ThenInclude(a => a.QuestionAnswer)
                    .ThenInclude(qa => qa.Question)
            .Include(p => p.Preferences)
            .Where(p => pendingIds.Contains(p.Id))
            .ToListAsync();

        var response = profiles.Select(p => p.ToResponse()).ToList();

        // Filter to show only main photo for all users
        foreach (var profile in response)
        {
            if (profile.ProfilePhotos != null && profile.ProfilePhotos.Any())
            {
                var mainPhoto = profile.ProfilePhotos.FirstOrDefault(p => p.IsMain);
                profile.ProfilePhotos = mainPhoto != null 
                    ? new List<UserProfilePhotoResponse> { mainPhoto }
                    : new List<UserProfilePhotoResponse>();
            }
        }

        // 4. Mask data if user is Free
        if (userProps.SubscriptionPlan == SubscriptionPlan.Free)
        {
            foreach (var profile in response)
            {
                profile.Name = "*****";
                profile.Bio = "*****";
                profile.Hobbies = new List<HobbyResponse>();
                profile.UserProfileAnswers = new List<UserProfileAnswerResponse>();
                
                // Set blur flag for the main photo
                if (profile.ProfilePhotos != null && profile.ProfilePhotos.Any())
                {
                    foreach (var photo in profile.ProfilePhotos)
                    {
                        photo.IsBlurred = true;
                    }
                }
            }
        }

        return response;
    }

    private async Task<(RedisSwipeStatus Status, bool IsMatch, int OppositeSwipeType, bool TargetFound)> ExecuteSwipeScript(
        int limit, string today, Guid swiperId, SwipeRequest request)
    {
        var limitKey = $"user:{swiperId}:swipe-count:{today}";
        var swipeKey = $"user:{swiperId}:swipes:{(int)request.Mode}";
        
        // We always construct targetSwipeKey now because even on Dislike, 
        // we want to check for a 'Missed Match'
        var targetSwipeKey = $"user:{request.TargetUserId}:swipes:{(int)request.Mode}";

        var result = (RedisResult[])await _db.ScriptEvaluateAsync(SwipeLuaScript,
            new RedisKey[] { limitKey, swipeKey, targetSwipeKey },
            new RedisValue[] { limit, request.TargetUserId.ToString(), (int)request.SwipeType, swiperId.ToString() });

        return (
            Status: (RedisSwipeStatus)(int)result[0],
            IsMatch: (int)result[1] == 1,
            OppositeSwipeType: (int)result[2],
            TargetFound: (int)result[3] == 1
        );
    }
}
