using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Models.Swipe.Requests;

public class SwipeRequest
{
    public int TargetUserId { get; set; }
    public SwipeType SwipeType { get; set; }
    public string DiscoveryToken { get; set; }
    public Mode Mode { get; set; }
}

