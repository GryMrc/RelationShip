namespace RelationshipService.Application.Models.Auth.Requests
{
    public class SocialLoginRequest
    {
        public string Token { get; set; } = null!;
        public string Provider { get; set; } = null!; // Google, Apple
    }
}
