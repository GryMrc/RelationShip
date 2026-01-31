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

        // 1. Persist the Swipe (Idempotency check for Redis failures)
        var existingSwipe = await context.Swipes
            .AsNoTracking()
            .AnyAsync(s => s.SwiperUserId == @event.SwiperUserId && s.SwipedUserId == @event.SwipedUserId);

        bool isNewSwipe = !existingSwipe;
        if (isNewSwipe)
        {
            var swipe = new Swipe(
                @event.SwiperUserId,
                @event.SwipedUserId,
                @event.SwipeType
            );

            context.Swipes.Add(swipe);
        }

        // 2. Definitive Match Check
        if (@event.DbCheckRequired)
        {
            Swipe? oppositeSwipe = null;
            // We fetch the full swipe to know if it's a Like, Dislike, or Nothing
            oppositeSwipe = await context.Swipes
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.SwiperUserId == @event.SwipedUserId &&
                    s.SwipedUserId == @event.SwiperUserId);

            if(oppositeSwipe is not null)
            {
               if (oppositeSwipe.SwipeType != SwipeType.Dislike)
                {
                    isDbMatch = true;
                }
            }
            else if (isNewSwipe && @event.SwipeType != SwipeType.Dislike)
            {
                // New Like Notification: Only if it's a first-time Like and target hasn't swiped back yet
                pendingNotification = new RelationshipActionResultEvent
                {
                    UserAId = @event.SwipedUserId,
                    UserBId = null,
                    Type = RelationshipNotificationType.NewLike,
                    MatchMode = @event.MatchMode
                };
            }
        }

        if (isDbMatch || @event.IsMatch)
        {
            //To do : burasi her zaman kucuk id buyuk id seklinde kaydetmeli
            var userAId = Math.Min(@event.SwiperUserId, @event.SwipedUserId);
            var userBId = Math.Max(@event.SwiperUserId, @event.SwipedUserId);

            var existingMatch = await context.Matches
                .AsNoTracking()
                .AnyAsync(m => m.UserAId == userAId && m.UserBId == userBId);

            if (!existingMatch)
            {
                var match = new Match(
                    userAId,
                    userBId,
                    @event.MatchMode
                );

                context.Matches.Add(match);

                // Match Notification: Trigger only on first creation
                pendingNotification = new RelationshipActionResultEvent
                {
                    UserAId = @event.SwipedUserId, // Target always gets notification
                    UserBId = @event.IsMatch ? null : @event.SwiperUserId, // Swiper only gets notif if it was a DB-discovered match
                    Type = RelationshipNotificationType.NewMatch,
                    MatchMode = @event.MatchMode
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
                        "Notification published successfully. Type: {NotificationType}, UserA: {UserAId}, UserB: {UserBId}, MatchMode: {MatchMode}",
                        pendingNotification.Type,
                        pendingNotification.UserAId,
                        pendingNotification.UserBId,
                        pendingNotification.MatchMode);
                }
                catch (Exception publishEx)
                {
                    // Critical: DB commit succeeded but notification publish failed
                    // Data is persisted but user won't receive notification
                    // Log as error for monitoring/alerting - may need manual intervention or reconciliation job
                    logger.LogError(publishEx,
                        "CRITICAL: Database commit succeeded but notification publish failed. " +
                        "Type: {NotificationType}, UserA: {UserAId}, UserB: {UserBId}, MatchMode: {MatchMode}, " +
                        "SwiperUserId: {SwiperUserId}, SwipedUserId: {SwipedUserId}. " +
                        "Data is persisted but notification was not sent. Consider implementing outbox pattern or reconciliation job.",
                        pendingNotification.Type,
                        pendingNotification.UserAId,
                        pendingNotification.UserBId,
                        pendingNotification.MatchMode,
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

