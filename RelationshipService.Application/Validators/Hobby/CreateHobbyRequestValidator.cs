using FluentValidation;
using RelationshipService.Application.Models.Hobby.Requests;

namespace RelationshipService.Application.Validators.Hobby;

public class CreateHobbyRequestValidator : AbstractValidator<CreateHobbyRequest>
{
    public CreateHobbyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Hobby name is required.")
            .MaximumLength(100).WithMessage("Hobby name cannot exceed 100 characters.");
    }
}
