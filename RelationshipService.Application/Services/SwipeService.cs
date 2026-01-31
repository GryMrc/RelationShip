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

    public async Task<SwipeResponse> SwipeAsync(int swiperId, SwipeRequest request)
    {
        // 1. Validate Token (Quick cryptographic check)
        if (!tokenService.ValidateToken(swiperId, request.TargetUserId, request.MatchMode, request.DiscoveryToken, out var plan))
        {
            throw new Exception("Invalid or expired discovery token for this mode");
        }

        bool isMatch = false;
        SwipeType? matchedSwipeType = null;
        bool dbCheckRequired = false;

        // Create a short-lived timeout for Redis operations
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

        try
        {
            // 2. Daily Limit Check (Atomic INCR)
            var limit = configuration.GetValue<int>($"SwipeSettings:DailyLimits:{plan}");
            if (limit != -1)
            {
                var today = DateTime.UtcNow.ToString("yyyyMMdd");
                var limitKey = $"user:{swiperId}:swipe-count:{today}";
                
                var newValue = await _db.StringIncrementAsync(limitKey); 
                if (newValue == 1)
                {
                    await _db.KeyExpireAsync(limitKey, TimeSpan.FromHours(24), CommandFlags.FireAndForget);
                }

                if (newValue > limit)
                {
                    throw new Exception($"You have reached your daily swipe limit for {plan} plan.");
                }
            }

            var swipeKey = $"user:{swiperId}:swipes:{(int)request.MatchMode}";

            // 3. Duplicate Check & Store (Atomic HashSet with When.NotExists)
            var wasSet = await _db.HashSetAsync(swipeKey, request.TargetUserId.ToString(), (int)request.SwipeType, When.NotExists);
            if (!wasSet)
            {
                throw new Exception("You have already swiped on this user in this mode");
            }
            
            await _db.KeyExpireAsync(swipeKey, TimeSpan.FromHours(48), CommandFlags.FireAndForget);

            // 4. Optimistic Match Check: Check if the target user has already liked us IN REDIS
            if (request.SwipeType != SwipeType.Dislike)
            {
                var targetSwipeKey = $"user:{request.TargetUserId}:swipes:{(int)request.MatchMode}";
                var targetSwipeValue = await _db.HashGetAsync(targetSwipeKey, swiperId.ToString());
                
                if (targetSwipeValue.HasValue)
                {
                    matchedSwipeType = (SwipeType)(int)targetSwipeValue;
                    if (matchedSwipeType != SwipeType.Dislike)
                    {
                        isMatch = true;
                    }
                }
                else
                {
                    dbCheckRequired = true;
                }
            }
        }
        catch (RedisTimeoutException ex)
        {
            logger.LogWarning(ex, "Redis Timeout (>{Timeout}ms) during SwipeAsync for user {SwiperId}. Entering Degraded Mode.", 500, swiperId);
            dbCheckRequired = true;
        }
        catch (RedisConnectionException ex)
        {
            logger.LogError(ex, "Redis Connection Failure during SwipeAsync for user {SwiperId}. Entering Degraded Mode.", swiperId);
            dbCheckRequired = true;
        }
        catch (RedisException ex)
        {
            logger.LogError(ex, "Unexpected Redis Error during SwipeAsync for user {SwiperId}. Entering Degraded Mode.", swiperId);
            dbCheckRequired = true; 
        }

        // 5. Fire and Forget: Let the Consumer handle persistence and definitive matching
        // RabbitMQ is our reliable backbone. Even if Redis is down, we don't lose the swipe.
        await publishEndpoint.Publish(new SwipeEvent
        {
            SwiperUserId = swiperId,
            SwipedUserId = request.TargetUserId,
            SwipeType = request.SwipeType,
            MatchMode = request.MatchMode,
            IsMatch = isMatch,
            DbCheckRequired = request.SwipeType != SwipeType.Dislike && dbCheckRequired
        });

        return new SwipeResponse 
        { 
            IsMatch = isMatch,
            SwipeType = isMatch ? request.SwipeType : null,
            MatchedSwipeType = isMatch ? matchedSwipeType : null
        };
    }
}
