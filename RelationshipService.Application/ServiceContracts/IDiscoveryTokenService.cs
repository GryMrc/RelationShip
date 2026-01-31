using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.ServiceContracts;

public interface IDiscoveryTokenService
{
    string GenerateToken(int swiperId, int swipedId, Mode mode, SubscriptionPlan plan);
    bool ValidateToken(int swiperId, int swipedId, Mode mode, string token, out SubscriptionPlan plan);
}


