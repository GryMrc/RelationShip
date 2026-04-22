using System.Text.Json.Serialization;

namespace RelationshipService.Application.Models.Profile.Requests;

public class UpsertProfileDeviceRequest
{
    public string Token { get; set; } = null!;

    public string DeviceId { get; set; }

    public string Name { get; set; }

    public string Platform { get; set; }
}
