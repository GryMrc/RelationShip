namespace RelationshipService.Application.Models.Auth
{
    public class SocialLoginRequest
    {
        public string Token { get; set; } = null!;
        public string Provider { get; set; } = null!; // Google, Apple
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = null!;
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime AccessTokenExpiresAt { get; set; }
    }
}
