using System.Net;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RelationshipService.Application.Models.Auth.Requests;
using RelationshipService.Application.Models.Auth.Responses;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Domain.Entities;
using RelationshipService.Domain.Enums;
using RelationshipService.Domain.Models;

namespace RelationshipService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IRelationShipDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthService(IRelationShipDbContext context, IJwtService jwtService, IConfiguration configuration)
    {
        _context = context;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<IResult<AuthResponse>> LoginWithSocial(SocialLoginRequest request)
    {
        string email;
        string providerKey;

        if (request.Provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Authentication:Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token, settings);
                email = payload.Email;
                providerKey = payload.Subject;
            }
            catch (Exception ex)
            {
                return Result<AuthResponse>.Failure(new Error(ErrorCode.Auth_InvalidGoogleToken), HttpStatusCode.BadRequest);
            }
        }
        else if (request.Provider.Equals("Apple", StringComparison.OrdinalIgnoreCase))
        {
            // TODO: Implement Apple Token Verification
            // For now, returning failure until Apple certificates/ClientSecret are configured
            return Result<AuthResponse>.Failure(new Error(ErrorCode.Auth_AppleAuthNotImplemented), HttpStatusCode.BadRequest);
        }
        else
        {
            return Result<AuthResponse>.Failure(new Error(ErrorCode.Auth_UnsupportedProvider), HttpStatusCode.BadRequest);
        }

        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Provider == request.Provider && u.ProviderKey == providerKey);

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Provider = request.Provider,
                ProviderKey = providerKey,
                Role = Roles.User
            };
            _context.Users.Add(user);
        }
        else
        {
            // Update email if changed
            user.Email = email;
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7")),
            CreatedDate = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return Result<AuthResponse>.Success(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60"))
        });
    }

    public async Task<IResult<AuthResponse>> Refresh(RefreshTokenRequest request)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            return Result<AuthResponse>.Failure(new Error(ErrorCode.Auth_InvalidRefreshToken), HttpStatusCode.BadRequest);
        }

        // Rotate token: Revoke old, issue new
        refreshToken.IsRevoked = true;
        
        var user = refreshToken.User;
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshTokenValue = _jwtService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7")),
            CreatedDate = DateTime.UtcNow
        };

        refreshToken.ReplacedByToken = newRefreshTokenValue;
        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        return Result<AuthResponse>.Success(new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60"))
        });
    }

    public async Task<IResult<bool>> RevokeToken(string token)
    {
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        if (refreshToken == null) return Result<bool>.Failure(new Error(ErrorCode.Auth_TokenNotFound), HttpStatusCode.NotFound);

        refreshToken.IsRevoked = true;
        await _context.SaveChangesAsync();
        return Result<bool>.Success(true);
    }
}
