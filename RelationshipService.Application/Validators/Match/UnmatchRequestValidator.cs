using FluentValidation;
using RelationshipService.Application.Models.Match.Requests;

namespace RelationshipService.Application.Validators.Match;

public class UnmatchRequestValidator : AbstractValidator<UnmatchRequest>
{
    public UnmatchRequestValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(200).WithMessage("Reason cannot exceed 200 characters.");
    }
}
