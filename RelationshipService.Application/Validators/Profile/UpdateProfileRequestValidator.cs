using FluentValidation;
using RelationshipService.Application.Models.Profile.Requests;

namespace RelationshipService.Application.Validators.Profile;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Bio).MaximumLength(500);
    }
}
