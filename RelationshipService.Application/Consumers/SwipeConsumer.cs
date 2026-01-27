using MassTransit;
using RelationshipService.Application.Events;
using RelationshipService.Domain.Entities;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Consumers;

public class SwipeConsumer(IRelationShipDbContext context) : IConsumer<SwipeEvent>
{
    public async Task Consume(ConsumeContext<SwipeEvent> contextMessage)
    {
        var @event = contextMessage.Message;

        var swipe = new Swipe(
            @event.SwiperUserId,
            @event.SwipedUserId,
            @event.SwipeType
        );

        context.Swipes.Add(swipe);

        if (@event.IsMatch)
        {
            var match = new Match(@event.SwiperUserId, @event.SwipedUserId, @event.MatchMode);
            context.Matches.Add(match);
        }


        await context.SaveChangesAsync();
    }
}

