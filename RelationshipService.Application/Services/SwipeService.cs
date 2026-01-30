using Microsoft.Extensions.Configuration;
using MassTransit;
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
    IConfiguration configuration) : ISwipeService
{
    private readonly IDatabase _db = redis.GetDatabase();
    private bool _dbCheckRequired = false;

    public async Task<SwipeResponse> SwipeAsync(int swiperId, SwipeRequest request)
    {
        // 1. Validate Token (Quick cryptographic check)
        if (!tokenService.ValidateToken(swiperId, request.TargetUserId, request.MatchMode, request.DiscoveryToken, out var plan))
        {
            throw new Exception("Invalid or expired discovery token for this mode");
        }

        // 2. Daily Limit Check (Redis-First)
        var limit = configuration.GetValue<int>($"SwipeSettings:DailyLimits:{plan}");
        if (limit != -1)
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var limitKey = $"user:{swiperId}:swipe-count:{today}";
            
            var currentCount = await _db.StringGetAsync(limitKey);
            if (currentCount.HasValue && (int)currentCount >= limit)
            {
                throw new Exception($"You have reached your daily swipe limit for {plan} plan.");
            }

            var newValue = await _db.StringIncrementAsync(limitKey);
            if (newValue == 1)
            {
                await _db.KeyExpireAsync(limitKey, TimeSpan.FromHours(24), CommandFlags.FireAndForget);
            }
        }

        var swipeKey = $"user:{swiperId}:swipes:{(int)request.MatchMode}";

        // 3. Duplicate Check: Prevent multiple swipes on the same user in this mode
        var existingSwipe = await _db.HashGetAsync(swipeKey, request.TargetUserId.ToString());
        if (existingSwipe.HasValue)
        {
            throw new Exception("You have already swiped on this user in this mode");
        }

        // 4. Store current swipe in Redis for "Hot Match" window (TTL 48h)
        await _db.HashSetAsync(swipeKey, request.TargetUserId.ToString(), (int)request.SwipeType);
        await _db.KeyExpireAsync(swipeKey, TimeSpan.FromHours(48));

        // 5. Optimistic Match Check: Check if the target user has already liked us IN REDIS
        bool isMatch = false;
        SwipeType? matchedSwipeType = null;

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
                _dbCheckRequired = true;
            }
        }

        // 6. Fire and Forget: Let the Consumer handle persistence and definitive matching
        await publishEndpoint.Publish(new SwipeEvent
        {
            SwiperUserId = swiperId,
            SwipedUserId = request.TargetUserId,
            SwipeType = request.SwipeType,
            MatchMode = request.MatchMode,
            IsMatch = isMatch,
            DbCheckRequired = _dbCheckRequired
        });

        return new SwipeResponse 
        { 
            IsMatch = isMatch,
            SwipeType = isMatch ? request.SwipeType : null,
            MatchedSwipeType = isMatch ? matchedSwipeType : null
        };
    }
}
