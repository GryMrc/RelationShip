using Microsoft.EntityFrameworkCore;
using RelationshipService.Application.Models.Question.Requests;
using RelationshipService.Application.Models.Question.Responses;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Application.Services;

public class QuestionService(IRelationShipDbContext context) : IQuestionService
{
    public async Task<IEnumerable<QuestionResponse>> GetAllAsync()
    {
        return await context.Questions
            .AsNoTracking()
            .Include(q => q.QuestionAnswers)
            .Select(q => new QuestionResponse
            {
                Id = q.Id,
                Text = q.Text,
                Answers = q.QuestionAnswers
                    .Where(a => !a.IsDeleted)
                    .Select(a => new AnswerResponse
                    {
                        Id = a.Id,
                        Text = a.AnswerText
                    }).ToList()
            })
            .ToListAsync();
    }

    public async Task<QuestionResponse?> GetByIdAsync(int id)
    {
        var question = await context.Questions
            .Include(q => q.QuestionAnswers)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null) return null;

        return new QuestionResponse
        {
            Id = question.Id,
            Text = question.Text,
            Answers = question.QuestionAnswers
                .Where(a => !a.IsDeleted)
                .Select(a => new AnswerResponse
                {
                    Id = a.Id,
                    Text = a.AnswerText
                }).ToList()
        };
    }

    public async Task<QuestionResponse> CreateAsync(CreateQuestionRequest request)
    {
        var question = new Question
        {
            Text = request.Text,
            QuestionAnswers = request.Answers.Select(a => new QuestionAnswer
            {
                AnswerText = a
            }).ToList()
        };

        await context.Questions.AddAsync(question);
        await context.SaveChangesAsync();

        return new QuestionResponse
        {
            Id = question.Id,
            Text = question.Text,
            Answers = question.QuestionAnswers.Select(a => new AnswerResponse
            {
                Id = a.Id,
                Text = a.AnswerText
            }).ToList()
        };
    }

    public async Task<QuestionResponse?> UpdateAsync(UpdateQuestionRequest request)
    {
        var question = await context.Questions.FindAsync(request.Id);
        if (question == null) return null;

        question.Text = request.Text;
        
        context.Questions.Update(question);
        await context.SaveChangesAsync();

        return await GetByIdAsync(question.Id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var question = await context.Questions.FindAsync(id);
        if (question == null) return false;

        context.Questions.Remove(question);
        await context.SaveChangesAsync();
        return true;
    }
}
