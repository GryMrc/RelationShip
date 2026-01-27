namespace RelationshipService.Application.ServiceContracts;

public interface IDiscoveryTokenService
{
    string GenerateToken(int swiperId, int swipedId);
    bool ValidateToken(int swiperId, int swipedId, string token);
}
