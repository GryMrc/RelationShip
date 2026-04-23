using RelationshipService.Application.Models.Auth;
using RelationshipService.Domain.Models;

namespace RelationshipService.Application.ServiceContracts;

public interface IAuthService
{
    Task<IResult<AuthResponse>> LoginWithSocial(SocialLoginRequest request);
    Task<IResult<AuthResponse>> Refresh(RefreshTokenRequest request);
    Task<IResult<bool>> RevokeToken(string token);
}
