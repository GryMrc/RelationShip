using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RelationshipService.Application.Events;
using RelationshipService.Domain.Entities;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Consumers;

public class SwipeConsumer(IRelationShipDbContext context) : IConsumer<SwipeEvent>
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

        if (!existingSwipe)
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
            else
            {
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
            }

            pendingNotification = new RelationshipActionResultEvent
            {
                UserAId = @event.SwiperUserId,
                UserBId = @event.SwipedUserId,
                Type = RelationshipNotificationType.NewMatch,
                MatchMode = @event.MatchMode
            };
        }

        try
        {
            await context.SaveChangesAsync();
            
            // Push notification only AFTER successful database commit
            if (pendingNotification != null)
            {
                await contextMessage.Publish(pendingNotification);
            }
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
        {
            // Unique Violation: Data already exists, meaning notification was likely sent by a previous attempt/instance.
            // We skip the notification here to avoid duplicates.
        }
    }
}

