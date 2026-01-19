using Microsoft.AspNetCore.Mvc;
using RelationshipService.Application.Models.UserProfile.Requests;
using RelationshipService.Application.ServiceContracts;

namespace RelationshipService.Api.Controller;

public class UserProfileController(IUserProfileService userProfileService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var profile = await userProfileService.GetByUserIdAsync(UserId);
        if (profile == null) return NotFound();
        return Ok(profile);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var profiles = await userProfileService.GetAllAsync();
        return Ok(profiles);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserProfileRequest request)
    {
        request.UserId = UserId;
        await userProfileService.CreateAsync(request);
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateUserProfileRequest request)
    {
        try
        {
            request.UserId = UserId;
            await userProfileService.UpdateAsync(request);
            return NoContent();
        }
        catch (Exception ex) when (ex.Message == "Profile not found")
        {
            return NotFound();
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        await userProfileService.DeleteAsync(UserId);
        return NoContent();
    }
}
