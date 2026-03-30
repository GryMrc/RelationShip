using System.Security.Claims;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Application.ServiceContracts
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
