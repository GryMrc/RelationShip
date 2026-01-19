using Microsoft.AspNetCore.Mvc;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Application.Models.UserPrefences.Requests;

namespace RelationshipService.Api.Controller;

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
