using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using RelationshipService.Application.Models.Swipe.Requests;
using RelationshipService.Application.ServiceContracts;
using Microsoft.AspNetCore.Authorization;

namespace RelationshipService.Api.Controller.v1;

[Authorize]
[ApiVersion("1.0")]
public class SwipeController(ISwipeService swipeService) : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Post(SwipeRequest request)
    {
        var response = await swipeService.SwipeAsync(UserId, request);
        return Ok(response);
    }

    [HttpGet("likers")]
    public async Task<IActionResult> GetLikers()
    {
        var response = await swipeService.GetLikersAsync(UserId);
        return Ok(response);
    }
}
