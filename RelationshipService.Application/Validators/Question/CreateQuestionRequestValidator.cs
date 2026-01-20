using FluentValidation;
using RelationshipService.Application.Models.Question.Requests;

namespace RelationshipService.Application.Validators.Question;

public class CreateQuestionRequestValidator : AbstractValidator<CreateQuestionRequest>
{
    public CreateQuestionRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Question text is required.")
            .MaximumLength(1000).WithMessage("Question text cannot exceed 1000 characters.");

        RuleFor(x => x.Answers)
            .NotEmpty().WithMessage("At least one answer is required.")
            .Must(x => x.Count >= 2).WithMessage("A question must have at least 2 answers.");

        RuleForEach(x => x.Answers)
            .NotEmpty().WithMessage("Answer text cannot be empty.")
            .MaximumLength(500).WithMessage("Answer text cannot exceed 500 characters.");
    }
}
