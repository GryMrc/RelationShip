namespace RelationshipService.Application.Models.Notification;

public class PushNotificationRequest
{
    public string DeviceToken { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public Dictionary<string, string>? Data { get; set; } // Ekstra veriler icin
}
