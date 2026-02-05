using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using RelationshipService.Application.Models.Profile.Requests;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Application.Models.Profile.Responses;

namespace RelationshipService.Api.Controller.v1;

[ApiVersion("1.0")]
public class ProfileController(IProfileService userProfileService) : BaseController
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
    public async Task<IActionResult> Create(CreateProfileRequest request)
    {
        request.UserId = UserId;
        await userProfileService.CreateAsync(request);
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateProfileRequest request)
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

    [HttpGet("discovery")]
    public async Task<ActionResult<List<DiscoveryProfileResponse>>> GetDiscovery()
    {
        var profiles = await userProfileService.GetDiscoveryProfilesAsync(UserId);
        return Ok(profiles);
    }

    [HttpPut("hobbies")]
    public async Task<IActionResult> SyncHobbies(SyncHobbiesRequest request)
    {
        await userProfileService.SyncHobbiesAsync(UserId, request);
        return NoContent();
    }

    [HttpPut("answers")]
    public async Task<IActionResult> SyncAnswers(SyncAnswersRequest request)
    {
        await userProfileService.SyncAnswersAsync(UserId, request);
        return NoContent();
    }

    [HttpPost("photos")]
    public async Task<IActionResult> AddPhoto(AddProfilePhotoRequest request)
    {
        await userProfileService.AddPhotoAsync(UserId, request);
        return Ok();
    }

    [HttpDelete("photos/{photoId}")]
    public async Task<IActionResult> DeletePhoto(int photoId)
    {
        await userProfileService.DeletePhotoAsync(UserId, photoId);
        return NoContent();
    }

    [HttpPatch("photos/{photoId}/main")]
    public async Task<IActionResult> SetMainPhoto(int photoId)
    {
        await userProfileService.SetMainPhotoAsync(UserId, photoId);
        return NoContent();
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences(UpdateProfilePreferencesRequest request)
    {
        request.UserId = UserId;
        await userProfileService.UpdatePreferencesAsync(request);
        return NoContent();
    }
}
