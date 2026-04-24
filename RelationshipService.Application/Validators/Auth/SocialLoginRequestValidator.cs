using FluentValidation;
using RelationshipService.Application.Models.Auth.Requests;

namespace RelationshipService.Application.Validators.Auth;

public class SocialLoginRequestValidator : AbstractValidator<SocialLoginRequest>
{
    public SocialLoginRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");

        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required.")
            .Must(p => p.Equals("Google", StringComparison.OrdinalIgnoreCase) || p.Equals("Apple", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Provider must be either 'Google' or 'Apple'.");
    }
}
