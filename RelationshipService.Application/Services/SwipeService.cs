using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RelationshipService.Application.Events;
using RelationshipService.Application.Models.Swipe.Requests;
using RelationshipService.Application.Models.Swipe.Responses;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Domain.Enums;
using StackExchange.Redis;

namespace RelationshipService.Application.Services;

public class SwipeService(
    IDiscoveryTokenService tokenService,
    IConnectionMultiplexer redis,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration,
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

    public async Task<SwipeResponse> SwipeAsync(int swiperId, SwipeRequest request)
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
            SwiperUserId = swiperId,
            SwipedUserId = request.TargetUserId,
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

    private async Task<(RedisSwipeStatus Status, bool IsMatch, int OppositeSwipeType, bool TargetFound)> ExecuteSwipeScript(
        int limit, string today, int swiperId, SwipeRequest request)
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
