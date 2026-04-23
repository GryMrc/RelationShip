using MassTransit;
using Microsoft.Extensions.Logging;
using RelationshipService.Application.Events;
using Microsoft.EntityFrameworkCore;
using RelationshipService.Application.Models.Notification;
using RelationshipService.Application.ServiceContracts;

namespace RelationshipService.Application.Consumers;

public class NotificationConsumer : IConsumer<RelationshipActionResultEvent>
{
    private readonly IRelationShipDbContext _context;
    private readonly ILogger<NotificationConsumer> _logger;
    private readonly IPushNotificationService _pushNotificationService;

    public NotificationConsumer(IRelationShipDbContext context, ILogger<NotificationConsumer> logger, IPushNotificationService pushNotificationService)
    {
        _context = context;
        _logger = logger;
        _pushNotificationService = pushNotificationService;
    }

    public async Task Consume(ConsumeContext<RelationshipActionResultEvent> context)
    {
        var query = _context.UserProfileDevices
            .Where(pd => pd.ProfileId == context.Message.UserAId);

        if (context.Message.UserBId.HasValue) { 
            query = query.Where(pd => pd.ProfileId == context.Message.UserBId.Value);
        }

        var profileDevices = await query.AsNoTracking().ToListAsync();

        foreach (var device in profileDevices)
        {
            var body = context.Message.Type switch
            {
                RelationshipNotificationType.NewMatch => "You have a new match!",
                RelationshipNotificationType.NewLike => "Someone liked you!",
                RelationshipNotificationType.NewMessage => "You have a new message.",
                RelationshipNotificationType.MissedMatch => "You missed a match.",
                _ => "You have a new notification."
            };

            var title = context.Message.Type switch
            {
                RelationshipNotificationType.NewMatch => "New Match!",
                RelationshipNotificationType.NewLike => "New Like!",
                RelationshipNotificationType.NewMessage => "New Message!",
                RelationshipNotificationType.MissedMatch => "Missed Match!",
                _ => "New Notification!"
            };

            var notification = new PushNotificationRequest
            {
                Title = title,
                Body = body,
                Data = null,
                DeviceToken = device.Token
            };

            // Here you would implement the logic to send a notification to the device. with fcm or apns
            _logger.LogInformation("Sending notification to device {DeviceId} for user {UserId} with type {NotificationType}", device.DeviceId, device.ProfileId, context.Message.Type);
            
            await _pushNotificationService.SendNotificationAsync(notification);
        }
    }
}
