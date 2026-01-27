using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.ServiceContracts;

public interface IDiscoveryTokenService
{
    string GenerateToken(int swiperId, int swipedId, MatchMode mode, SubscriptionPlan plan);
    bool ValidateToken(int swiperId, int swipedId, MatchMode mode, string token, out SubscriptionPlan plan);
}


