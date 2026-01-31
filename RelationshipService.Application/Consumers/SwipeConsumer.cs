using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using RelationshipService.Application.Events;
using RelationshipService.Domain.Entities;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Consumers;

public class SwipeConsumer(IRelationShipDbContext context, ILogger<SwipeConsumer> logger) : IConsumer<SwipeEvent>
{
    public async Task Consume(ConsumeContext<SwipeEvent> contextMessage)
    {
        var @event = contextMessage.Message;
        bool isDbMatch = false;
        RelationshipActionResultEvent? pendingNotification = null;

        // 1. Fetch relevant swipes (Own and Reciprocal) in one go
        var swipes = await context.Swipes
            .AsNoTracking()
            .Where(s => ((s.SwiperUserId == @event.SwiperUserId && s.SwipedUserId == @event.SwipedUserId) ||
                         (s.SwiperUserId == @event.SwipedUserId && s.SwipedUserId == @event.SwiperUserId)) &&
                        s.Mode == @event.Mode)
            .ToListAsync();

        var existingSwipe = swipes.FirstOrDefault(s => s.SwiperUserId == @event.SwiperUserId);
        
        // Use Redis-discovered opposite swipe if available, otherwise fallback to DB result
        Swipe? oppositeSwipe = null;
        if (@event.OppositeSwipeType.HasValue)
        {
            // Create a dummy object to hold the swipe type discovered in Redis
            oppositeSwipe = new Swipe(@event.SwipedUserId, @event.SwiperUserId, (SwipeType)@event.OppositeSwipeType.Value, @event.Mode);
        }
        else
        {
            oppositeSwipe = swipes.FirstOrDefault(s => s.SwiperUserId == @event.SwipedUserId);
        }
        
        bool isNewSwipe = existingSwipe == null;
        if (isNewSwipe)
        {
            context.Swipes.Add(new Swipe(@event.SwiperUserId, @event.SwipedUserId, @event.SwipeType, @event.Mode));
        }

        // 2. Definitive Match Check
        // A match only exists if BOTH parties liked.
        if (@event.SwipeType != SwipeType.Dislike)
        {
            if (oppositeSwipe is { SwipeType: not SwipeType.Dislike })
            {
                isDbMatch = true;
            }
        }

        var matchDetected = isDbMatch || @event.IsRedisMatch;

        // 3. Notification & Match Lifecycle
        if (matchDetected == false && isNewSwipe)
        {
            // Case A: Ahmet liked, Ayşe unknown -> NewLike notification for Ayşe
            if (oppositeSwipe is null && @event.SwipeType != SwipeType.Dislike)
            {
                pendingNotification = new RelationshipActionResultEvent
                {
                    UserAId = @event.SwipedUserId,
                    UserBId = null,
                    Type = RelationshipNotificationType.NewLike,
                    Mode = @event.Mode
                };
            }
            // Case B: Ahmet DISLIKED, but Ayşe had already LIKED Ahmet -> MissedMatch notification for Ahmet
            else if (oppositeSwipe is { SwipeType: not SwipeType.Dislike } && @event.SwipeType == SwipeType.Dislike)
            {
                pendingNotification = new RelationshipActionResultEvent
                {
                    UserAId = @event.SwiperUserId,
                    UserBId = null,
                    Type = RelationshipNotificationType.MissedMatch,
                    Mode = @event.Mode
                };
            }
        }

        if (matchDetected)
        {
            // We double-verify here just in case, though isMatch should already be false if Dislike
            if (@event.SwipeType == SwipeType.Dislike) return; 

            var userAId = Math.Min(@event.SwiperUserId, @event.SwipedUserId);
            var userBId = Math.Max(@event.SwiperUserId, @event.SwipedUserId);

            var alreadyMatched = await context.Matches
                .AsNoTracking()
                .AnyAsync(m => m.UserAId == userAId && m.UserBId == userBId);

            if (!alreadyMatched)
            {
                var match = new Match(
                    userAId,
                    userBId,
                    @event.Mode
                );

                context.Matches.Add(match);

                // Match Notification: Trigger only on first creation
                pendingNotification = new RelationshipActionResultEvent
                {
                    UserAId = @event.SwipedUserId, // Target always gets notification
                    UserBId = @event.IsRedisMatch ? null : @event.SwiperUserId, // Swiper only gets notif if it was a DB-discovered match
                    Type = RelationshipNotificationType.NewMatch,
                    Mode = @event.Mode
                };
            }
        }

        try
        {
            // Step 1: Persist to database first
            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }

            // Step 2: Publish notification AFTER successful database commit
            // This ensures data consistency: if publish fails, data is still persisted
            // and can be recovered via a separate reconciliation process
            if (pendingNotification != null)
            {
                try
                {
                    await contextMessage.Publish(pendingNotification);
                    logger.LogInformation(
                        "Notification published successfully. Type: {NotificationType}, UserA: {UserAId}, UserB: {UserBId}, Mode: {Mode}",
                        pendingNotification.Type,
                        pendingNotification.UserAId,
                        pendingNotification.UserBId,
                        pendingNotification.Mode);
                }
                catch (Exception publishEx)
                {
                    // Critical: DB commit succeeded but notification publish failed
                    // Data is persisted but user won't receive notification
                    // Log as error for monitoring/alerting - may need manual intervention or reconciliation job
                    logger.LogError(publishEx,
                        "CRITICAL: Database commit succeeded but notification publish failed. " +
                        "Type: {NotificationType}, UserA: {UserAId}, UserB: {UserBId}, Mode: {Mode}, " +
                        "SwiperUserId: {SwiperUserId}, SwipedUserId: {SwipedUserId}. " +
                        "Data is persisted but notification was not sent. Consider implementing outbox pattern or reconciliation job.",
                        pendingNotification.Type,
                        pendingNotification.UserAId,
                        pendingNotification.UserBId,
                        pendingNotification.Mode,
                        @event.SwiperUserId,
                        @event.SwipedUserId);

                    // Note: We don't throw here because:
                    // 1. DB transaction already committed - can't rollback
                    // 2. Throwing would cause MassTransit retry, potentially creating duplicates
                    // 3. Better to log and handle via monitoring/reconciliation
                }
            }
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
        {
            // Unique Violation: Data already exists, meaning notification was likely sent by a previous attempt/instance.
            // We skip the notification here to avoid duplicates.
            logger.LogInformation(
                "Duplicate swipe detected (unique constraint violation). SwiperUserId: {SwiperUserId}, SwipedUserId: {SwipedUserId}. " +
                "This is expected during retries or concurrent processing.",
                @event.SwiperUserId,
                @event.SwipedUserId);
        }
    }
}

