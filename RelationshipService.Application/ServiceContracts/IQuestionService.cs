using RelationshipService.Application.Models.Question.Requests;
using RelationshipService.Application.Models.Question.Responses;

namespace RelationshipService.Application.ServiceContracts;

public interface IQuestionService
{
    Task<IEnumerable<QuestionResponse>> GetAllAsync();
    Task<QuestionResponse?> GetByIdAsync(int id);
    Task<QuestionResponse> CreateAsync(CreateQuestionRequest request);
    Task<QuestionResponse?> UpdateAsync(UpdateQuestionRequest request);
    Task<bool> DeleteAsync(int id);
}
