using RelationshipService.Application.Models.Notification;

namespace RelationshipService.Application.ServiceContracts;

public interface IPushNotificationService
{
    Task<bool> SendNotificationAsync(PushNotificationRequest request);
}
