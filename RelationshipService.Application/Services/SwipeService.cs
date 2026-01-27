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
    IPublishEndpoint publishEndpoint) : ISwipeService
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<SwipeResponse> SwipeAsync(int swiperId, SwipeRequest request)
    {
        // 1. Validate Token
        if (!tokenService.ValidateToken(swiperId, request.TargetUserId, request.DiscoveryToken))
        {
            throw new Exception("Invalid or expired discovery token");
        }

        // 2. Check for Match (Only if it's a Like or SuperLike)
        bool isMatch = false;
        if (request.SwipeType != SwipeType.Dislike)
        {
            // Check if the target user has already liked the current user in Redis
            var targetSwipeValue = await _db.HashGetAsync($"user:{request.TargetUserId}:swipes", swiperId.ToString());
            
            if (targetSwipeValue.HasValue && (int)targetSwipeValue != (int)SwipeType.Dislike)
            {
                isMatch = true;
            }
        }

        // 3. Store current swipe in Redis for instant lookup (TTL 48h)
        var swipeKey = $"user:{swiperId}:swipes";
        await _db.HashSetAsync(swipeKey, request.TargetUserId.ToString(), (int)request.SwipeType);
        await _db.KeyExpireAsync(swipeKey, TimeSpan.FromHours(48));

        // 4. Publish Event to RabbitMQ for DB persistence
        await publishEndpoint.Publish(new SwipeEvent
        {
            SwiperUserId = swiperId,
            SwipedUserId = request.TargetUserId,
            SwipeType = request.SwipeType,
            CreatedAt = DateTime.UtcNow
        });

        return new SwipeResponse { IsMatch = isMatch };
    }
}
