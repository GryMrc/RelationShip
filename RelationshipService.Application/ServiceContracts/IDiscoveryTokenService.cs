using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.ServiceContracts;

public interface IDiscoveryTokenService
{
    string GenerateToken(Guid swiperId, long swipedId, Mode mode, SubscriptionPlan plan);
    bool ValidateToken(Guid swiperId, long swipedId, Mode mode, string token, out SubscriptionPlan plan);
}


