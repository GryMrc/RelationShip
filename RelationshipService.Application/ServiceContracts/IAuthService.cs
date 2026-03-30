using RelationshipService.Application.Models.Auth;

namespace RelationshipService.Application.ServiceContracts
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginWithSocial(SocialLoginRequest request);
        Task<AuthResponse> Refresh(RefreshTokenRequest request);
        Task<bool> RevokeToken(string token);
    }
}
