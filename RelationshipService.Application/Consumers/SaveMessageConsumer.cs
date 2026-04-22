using MassTransit;
using Microsoft.Extensions.Logging;
using RelationshipService.Application.Events;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Application.Consumers;

public class SaveMessageConsumer(IRelationShipDbContext context, ILogger<SaveMessageConsumer> logger) : IConsumer<SaveMessageEvent>
{
    public async Task Consume(ConsumeContext<SaveMessageEvent> contextMessage)
    {
        var @event = contextMessage.Message;

        var message = new Message
        {
            MatchId = @event.MatchId,
            SenderProfileId = @event.SenderProfileId,
            ReceiverProfileId = @event.ReceiverProfileId,
            Content = @event.Content,
        };

        context.Messages.Add(message);

        try
        {
            await context.SaveChangesAsync();
            logger.LogInformation("Message saved successfully: MatchId {MatchId}, Sender {SenderId}", @event.MatchId, @event.SenderProfileId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while saving message for MatchId {MatchId}", @event.MatchId);
            throw;
        }
    }
}
