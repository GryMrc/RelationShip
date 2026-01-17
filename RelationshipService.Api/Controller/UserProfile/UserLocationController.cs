using Microsoft.AspNetCore.Mvc;
using RelationshipService.Application.Services.User;
using RelationshipService.Application.Models.UserLocation.Requests;

namespace RelationshipService.Api.Controller.User;

public class UserLocationController(IUserLocationService userLocationService) : BaseController
{
    [HttpPut]
    public async Task<IActionResult> Update(UpdateUserLocationRequest request)
    {
        request.UserId = UserId;
        await userLocationService.UpdateAsync(request);
        return NoContent();
    }
}
