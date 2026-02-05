using FluentValidation;
using RelationshipService.Application.Models.Profile.Requests;

namespace RelationshipService.Application.Validators.Profile;

public class CreateProfileRequestValidator : AbstractValidator<CreateProfileRequest>
{
    public CreateProfileRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Bio).MaximumLength(500);
        RuleFor(x => x.Gender).IsInEnum();
        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.Today.AddYears(-18))
            .WithMessage("You must be 18 years or older.");
    }
}
