using FluentValidation;
using RelationshipService.Application.Models.UserProfile.Requests;

namespace RelationshipService.Application.Validators.Profile;

public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Bio).MaximumLength(500);
        RuleFor(x => x.Gender).IsInEnum();
        RuleFor(x => x.DateOfBirth).NotEmpty().LessThan(DateTime.Now);
    }
}
