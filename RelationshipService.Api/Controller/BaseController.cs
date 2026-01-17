using Microsoft.AspNetCore.Mvc;

namespace RelationshipService.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    protected int UserId
    {
        get
        {
            if (Request.Headers.TryGetValue("X-User-Id", out var userIdStr) && int.TryParse(userIdStr, out var userId))
            {
                return userId;
            }
            return 0;
        }
    }
}
