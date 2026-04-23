using Microsoft.AspNetCore.Mvc;
using RelationshipService.Application.Models.Auth;
using RelationshipService.Application.ServiceContracts;
using Asp.Versioning;

namespace RelationshipService.Api.Controller.v1;

[ApiVersion("1.0")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("social-login")]
    public async Task<IActionResult> SocialLogin([FromBody] SocialLoginRequest request)
    {
        return Ok(await _authService.LoginWithSocial(request));
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var response = await _authService.Refresh(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RevokeToken(request.RefreshToken);
        if (!result) return NotFound();
        return Ok();
    }
}
