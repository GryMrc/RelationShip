using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelationshipService.Application.Models.Question.Requests;
using RelationshipService.Application.Models.Question.Responses;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Api.Controller.v1;

[Authorize]
[ApiVersion("1.0")]
public class QuestionController(IQuestionService questionService) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuestionResponse>>> GetAll()
    {
        var result = await questionService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QuestionResponse>> GetById(int id)
    {
        var result = await questionService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    [HttpPost]
    public async Task<ActionResult<QuestionResponse>> Create(CreateQuestionRequest request)
    {
        var result = await questionService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    [HttpPut("{id}")]
    public async Task<ActionResult<QuestionResponse>> Update(int id, UpdateQuestionRequest request)
    {
        if (id != request.Id) return BadRequest();
        
        var result = await questionService.UpdateAsync(request);
        if (result == null) return NotFound();
        
        return Ok(result);
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await questionService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
