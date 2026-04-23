using FirebaseAdmin.Messaging;
using RelationshipService.Application.Models.Notification;
using RelationshipService.Application.ServiceContracts;
using Message = FirebaseAdmin.Messaging.Message;

namespace RelationshipService.Application.Services;

public class FcmService : IPushNotificationService
{
    public async Task<bool> SendNotificationAsync(PushNotificationRequest request)
    {
        var message = new Message()
        {
            Token = request.DeviceToken,
            Notification = new Notification()
            {
                Title = request.Title,
                Body = request.Body
            },
            Data = request.Data,
            // iOS spesifik ayarlar (ileride burayı genişletebilirsin)
            //Apns = new ApnsConfig()
            //{
            //    Headers = new Dictionary<string, string>() { { "apns-priority", "10" } },
            //    Payload = new ApnsPayload() { Aps = new Aps() { Badge = 1 } }
            //}
        };

        try
        {
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return !string.IsNullOrEmpty(response);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
