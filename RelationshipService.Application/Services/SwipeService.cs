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

    public async Task<SwipeResponse> SwipeAsync(int swiperId, SwipeRequest request)
    {
        // 1. Validate Token (Mode-Aware & Plan-Aware Security)
        // Since the token is cryptographically signed with the mode, we don't need a DB hit here.
        if (!tokenService.ValidateToken(swiperId, request.TargetUserId, request.MatchMode, request.DiscoveryToken, out var plan))
        {
            throw new Exception("Invalid or expired discovery token for this mode");
        }

        // 2. Daily Limit Check (Redis-First)
        var limit = configuration.GetValue<int>($"SwipeSettings:DailyLimits:{plan}");
        
        // If limit is not -1 (Unlimited), check current count
        if (limit != -1)
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var limitKey = $"user:{swiperId}:swipe-count:{today}";
            
            var currentCount = await _db.StringGetAsync(limitKey);
            
            if (currentCount.HasValue && (int)currentCount >= limit)
            {
                throw new Exception($"You have reached your daily swipe limit for {plan} plan.");
            }

            // Increment and set expiry (24h) only on first increment
            var newValue = await _db.StringIncrementAsync(limitKey);
            if (newValue == 1)
            {
                await _db.KeyExpireAsync(limitKey, TimeSpan.FromHours(24), CommandFlags.FireAndForget);
            }
        }

        var swipeKey = $"user:{swiperId}:swipes:{(int)request.MatchMode}";

        // 2. Duplicate Check: Prevent multiple swipes on the same user in this mode
        var existingSwipe = await _db.HashGetAsync(swipeKey, request.TargetUserId.ToString());
        if (existingSwipe.HasValue)
        {
            throw new Exception("You have already swiped on this user in this mode");
        }

        // 3. Store current swipe in Redis for this mode (TTL 48h)
        await _db.HashSetAsync(swipeKey, request.TargetUserId.ToString(), (int)request.SwipeType);
        await _db.KeyExpireAsync(swipeKey, TimeSpan.FromHours(48));

        // 4. Check for Match (Only if it's a Like or SuperLike in the same mode)
        bool isMatch = false;
        SwipeType? matchedSwipeType = null;

        if (request.SwipeType != SwipeType.Dislike)
        {
            // Check if the target user has already liked the current user IN THE SAME MODE
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
        }

        // 5. Publish Event to RabbitMQ for DB persistence
        await publishEndpoint.Publish(new SwipeEvent
        {
            SwiperUserId = swiperId,
            SwipedUserId = request.TargetUserId,
            SwipeType = request.SwipeType,
            MatchMode = request.MatchMode,
            IsMatch = isMatch,
            CreatedAt = DateTime.UtcNow
        });



        return new SwipeResponse 
        { 
            IsMatch = isMatch,
            SwipeType = isMatch ? request.SwipeType : null,
            MatchedSwipeType = isMatch ? matchedSwipeType : null
        };
    }
}
