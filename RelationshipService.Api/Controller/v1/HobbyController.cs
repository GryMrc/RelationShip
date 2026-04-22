using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using RelationshipService.Application.Models.Hobby.Requests;
using RelationshipService.Application.Models.Hobby.Responses;
using RelationshipService.Application.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Api.Controller.v1;

[Authorize]
[ApiVersion("1.0")]
public class HobbyController(IHobbyService hobbyService) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HobbyResponse>>> GetAll()
    {
        var result = await hobbyService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HobbyResponse>> GetById(int id)
    {
        var result = await hobbyService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    [HttpPost]
    public async Task<ActionResult<HobbyResponse>> Create(CreateHobbyRequest request)
    {
        var result = await hobbyService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    [HttpPut("{id}")]
    public async Task<ActionResult<HobbyResponse>> Update(int id, UpdateHobbyRequest request)
    {
        if (id != request.Id) return BadRequest();
        
        var result = await hobbyService.UpdateAsync(request);
        if (result == null) return NotFound();
        
        return Ok(result);
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await hobbyService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
