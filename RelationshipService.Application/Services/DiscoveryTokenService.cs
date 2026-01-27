using System.Security.Cryptography;
using System.Text;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Services;

public class DiscoveryTokenService : IDiscoveryTokenService
{
    private const string SecretKey = "your-very-secure-secret-key-change-me-later";

    public string GenerateToken(int swiperId, int swipedId, MatchMode mode, SubscriptionPlan plan)
    {
        var expiry = DateTime.UtcNow.AddHours(24).Ticks;
        var payload = $"{swiperId}:{swipedId}:{(int)mode}:{(int)plan}:{expiry}";
        var key = Encoding.UTF8.GetBytes(SecretKey);

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var signature = Convert.ToBase64String(hash);

        return $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(payload))}.{signature}";
    }

    public bool ValidateToken(int swiperId, int swipedId, MatchMode mode, string token, out SubscriptionPlan plan)
    {
        plan = SubscriptionPlan.Free;
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 2) return false;

            var payloadBase64 = parts[0];
            var signature = parts[1];
            var payload = Encoding.UTF8.GetString(Convert.FromBase64String(payloadBase64));
            
            var payloadParts = payload.Split(':');
            if (payloadParts.Length != 5) return false;

            if (!int.TryParse(payloadParts[0], out var tokenSwiperId) || tokenSwiperId != swiperId) return false;
            if (!int.TryParse(payloadParts[1], out var tokenSwipedId) || tokenSwipedId != swipedId) return false;
            if (!int.TryParse(payloadParts[2], out var tokenMode) || tokenMode != (int)mode) return false;
            if (!int.TryParse(payloadParts[3], out var tokenPlan)) return false;
            if (!long.TryParse(payloadParts[4], out var expiryTicks)) return false;

            plan = (SubscriptionPlan)tokenPlan;

            if (DateTime.UtcNow.Ticks > expiryTicks) return false;

            // Verify signature
            var key = Encoding.UTF8.GetBytes(SecretKey);
            using var hmac = new HMACSHA256(key);
            var expectedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expectedSignature = Convert.ToBase64String(expectedHash);

            return signature == expectedSignature;
        }
        catch
        {
            return false;
        }
    }

}

