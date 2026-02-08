using MassTransit;
using Microsoft.Extensions.Logging;
using RelationshipService.Application.Events;
using RelationshipWebSocketServer.Services;

namespace RelationshipWebSocketServer.Consumers
{
    public class MatchEventConsumer(IClientConnectionManager connectionManager, ILogger<MatchEventConsumer> logger) : IConsumer<RelationshipActionResultEvent>
    {
        public async Task Consume(ConsumeContext<RelationshipActionResultEvent> context)
        {
            var @event = context.Message;

            if (@event.Type == RelationshipNotificationType.NewMatch)
            {
                if (@event.UserBId.HasValue && @event.MatchId.HasValue)
                {
                    UpdateUserMatches(@event.UserAId, @event.UserBId.Value, @event.MatchId.Value);
                    UpdateUserMatches(@event.UserBId.Value, @event.UserAId, @event.MatchId.Value);

                    logger.LogInformation("Match Added to Connections: {UserA} <-> {UserB} with MatchId {MatchId}", @event.UserAId, @event.UserBId.Value, @event.MatchId.Value);
                }
            }
            else if (@event.Type == RelationshipNotificationType.Unmatch)
            {
                if (@event.UserBId.HasValue)
                {
                    connectionManager.RemoveMatch(@event.UserAId, @event.UserBId.Value);
                    connectionManager.RemoveMatch(@event.UserBId.Value, @event.UserAId);
                    
                    logger.LogInformation("Match Removed from Connections: {UserA} <-> {UserB}", @event.UserAId, @event.UserBId.Value);
                }
            }
            
            await Task.CompletedTask;
        }

        private void UpdateUserMatches(long profileId, long matchedProfileId, long matchId)
        {
            var connections = connectionManager.GetClientsByProfileId(profileId);
            foreach (var client in connections)
            {
                if (!client.Matches.Any(m => m.MatchedProfileId == matchedProfileId))
                {
                    client.Matches.Add(new RelationshipService.Domain.Models.MatchSyncItem 
                    { 
                        MatchedProfileId = matchedProfileId, 
                        MatchId = matchId 
                    });
                }
            }
        }
    }
}
