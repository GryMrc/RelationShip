using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using MassTransit;
using RelationshipService.Application.Events;
using Microsoft.Extensions.Logging;
using RelationshipWebSocketServer.Services;
using System.Collections.Concurrent;
using RelationshipService.Domain.Models;

namespace RelationshipWebSocketServer.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IClientConnectionManager _connectionManager;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(
            IPublishEndpoint publishEndpoint, 
            IHttpClientFactory httpClientFactory,
            IClientConnectionManager connectionManager,
            ILogger<ChatHub> logger)
        {
            _publishEndpoint = publishEndpoint;
            _httpClientFactory = httpClientFactory;
            _connectionManager = connectionManager;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userIdStr = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                Context.Abort();
                return;
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RelationshipApi");
                client.DefaultRequestHeaders.Add("X-User-Id", userIdStr);

                var response = await client.GetAsync("/api/v1/match/socket-init");
                if (response.IsSuccessStatusCode)
                {
                    var state = await response.Content.ReadFromJsonAsync<UserMatchState>();
                    if (state != null)
                    {
                        var httpContext = Context.GetHttpContext();
                        var socketClient = new SocketClient
                        {
                            ConnectionId = Context.ConnectionId,
                            UserId = userId,
                            ProfileId = state.ProfileId,
                            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                            UserAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown",
                            ConnectedAt = DateTime.UtcNow,
                            Matches = state.Matches
                        };

                        _connectionManager.AddClient(socketClient);

                        // Join groups for real-time delivery
                        await Groups.AddToGroupAsync(Context.ConnectionId, state.ProfileId.ToString());
                        _logger.LogInformation("Profile {ProfileId} connected from {IP} ({UA})", state.ProfileId, socketClient.IpAddress, socketClient.UserAgent);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to sync state for user {UserId}. StatusCode: {StatusCode}", userId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OnConnectedAsync for user {UserId}", userId);
            }

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(long targetProfileId, string content)
        {
            var socketClient = _connectionManager.GetClient(Context.ConnectionId);
            if (socketClient == null) return;

            var matchInfo = _connectionManager.GetMatchInfo(socketClient.ProfileId, targetProfileId);
            if (!matchInfo.IsMatched)
            {
                _logger.LogWarning("Unauthorized message attempt from {SenderId} to {TargetId}", socketClient.ProfileId, targetProfileId);
                return;
            }

            // Real-time broadcast to all devices of the recipient
            await Clients.Group(targetProfileId.ToString()).SendAsync("ReceiveMessage", socketClient.ProfileId, content);

            // 3. Persist to DB via RabbitMQ
            await _publishEndpoint.Publish(new SaveMessageEvent
            {
                MatchId = matchInfo.MatchId,
                SenderProfileId = socketClient.ProfileId,
                ReceiverProfileId = targetProfileId,
                Content = content,
            });
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _connectionManager.RemoveClient(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
