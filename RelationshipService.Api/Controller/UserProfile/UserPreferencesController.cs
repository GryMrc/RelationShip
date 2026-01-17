using Microsoft.AspNetCore.Mvc;
using RelationshipService.Application.Services.User;
using RelationshipService.Application.Models.UserPrefences.Requests;

namespace RelationshipService.Api.Controller.User;

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
