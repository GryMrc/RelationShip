using MassTransit;
using Microsoft.EntityFrameworkCore;
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

        // 1. Persist the Swipe (Always)
        var swipe = new Swipe(
            @event.SwiperUserId,
            @event.SwipedUserId,
            @event.SwipeType
        );

        context.Swipes.Add(swipe);

        // 2. Definitive Match Check: If it's a Like/SuperLike, check if reciprocal exists in DB
        if (@event.DbCheckRequired)
        {
            isDbMatch = await context.Swipes.AsNoTracking().AnyAsync(s =>
            s.SwiperUserId == @event.SwipedUserId &&
            s.SwipedUserId == @event.SwiperUserId &&
            s.IsLiked != SwipeType.Dislike);
        }

        if (isDbMatch || @event.IsMatch)
        {
            var match = new Match(@event.SwiperUserId, @event.SwipedUserId, @event.MatchMode);
            context.Matches.Add(match);
        }

        await context.SaveChangesAsync();

        //push notification logic can be added here in future
    }
}

