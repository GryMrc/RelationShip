using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelationshipService.Application.Models.Match.Requests;
using RelationshipService.Application.ServiceContracts;

namespace RelationshipService.Api.Controller.v1;

[Authorize]
[ApiVersion("1.0")]
public class MatchController(IMatchService matchService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetMatches([FromQuery] GetMatchesRequest request)
    {
        var matches = await matchService.GetMatchesAsync(UserId, request);
        return Ok(matches);
    }

    [HttpGet("socket-init")]
    public async Task<IActionResult> GetUserMatchState()
    {
        var state = await matchService.GetUserMatchStateAsync(UserId);
        return Ok(state);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Unmatch(int id, [FromBody] UnmatchRequest request)
    {
        await matchService.UnmatchAsync(UserId, id, request.Reason ?? string.Empty);
        return Ok();
    }
}
