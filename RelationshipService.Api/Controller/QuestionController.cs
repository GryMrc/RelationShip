using Microsoft.AspNetCore.Mvc;
using RelationshipService.Application.Models.Question.Requests;
using RelationshipService.Application.Models.Question.Responses;
using RelationshipService.Application.ServiceContracts;

namespace RelationshipService.Api.Controller;

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

    [HttpPost]
    public async Task<ActionResult<QuestionResponse>> Create(CreateQuestionRequest request)
    {
        var result = await questionService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<QuestionResponse>> Update(int id, UpdateQuestionRequest request)
    {
        if (id != request.Id) return BadRequest();
        
        var result = await questionService.UpdateAsync(request);
        if (result == null) return NotFound();
        
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await questionService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
