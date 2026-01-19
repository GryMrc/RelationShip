using FluentValidation;
using RelationshipService.Application.Models.UserProfile.Requests;

namespace RelationshipService.Application.Validators.UserProfile;

public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {
        RuleFor(x => x.Bio).MaximumLength(500);
    }
}
