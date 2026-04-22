using MassTransit;
using RelationshipService.Application.Events;

namespace RelationshipService.Application.Consumers;

public class NotificationConsumer : IConsumer<RelationshipActionResultEvent>
{
    public Task Consume(ConsumeContext<RelationshipActionResultEvent> context)
    {
        throw new NotImplementedException();
    }
}
