using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RelationshipService.Application.ServiceContracts;

using RelationshipService.Application.Models.Match.Requests;

namespace RelationshipService.Api.Controller.v1;

[ApiVersion("1.0")]
public class MatchController(IMatchService matchService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetMatches([FromQuery] GetMatchesRequest request)
    {
        var matches = await matchService.GetMatchesAsync(UserId, request);
        return Ok(matches);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Unmatch(int id, [FromBody] UnmatchRequest request)
    {
        await matchService.UnmatchAsync(UserId, id, request.Reason ?? string.Empty);
        return Ok();
    }
}
