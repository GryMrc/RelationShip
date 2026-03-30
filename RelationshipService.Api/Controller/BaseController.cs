using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace RelationshipService.Api.Controller;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class BaseController : ControllerBase
{
    protected Guid UserId
    {
        get
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated or user id claim is missing.");
            }
            return userId;
        }
    }
}
