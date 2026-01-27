using MassTransit;
using RelationshipService.Application.Events;
using RelationshipService.Domain.Entities;

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
        await context.SaveChangesAsync();
    }
}
