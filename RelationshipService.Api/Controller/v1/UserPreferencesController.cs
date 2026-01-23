using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Application.Models.UserPrefences.Requests;

namespace RelationshipService.Api.Controller.v1;

[ApiVersion("1.0")]
public class UserPreferencesController(IUserPreferencesService userPreferencesService) : BaseController
{
    [HttpPut]
    public async Task<IActionResult> Update(UpdateUserPreferencesRequest request)
    {
        request.UserId = UserId;
        await userPreferencesService.UpdateAsync(request);
        return NoContent();
    }
}
