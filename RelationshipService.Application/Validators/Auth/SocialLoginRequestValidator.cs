using FluentValidation;
using RelationshipService.Application.Models.Auth.Requests;
using RelationshipService.Domain.Enums;
using RelationshipService.Application.Extensions;

namespace RelationshipService.Application.Validators.Auth;

public class SocialLoginRequestValidator : AbstractValidator<SocialLoginRequest>
{
    public SocialLoginRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithError(ErrorCode.Auth_TokenRequired);

        RuleFor(x => x.Provider)
            .NotEmpty().WithError(ErrorCode.Auth_ProviderRequired)
            .Must(p => p.Equals("Google", StringComparison.OrdinalIgnoreCase) || p.Equals("Apple", StringComparison.OrdinalIgnoreCase))
            .WithError(ErrorCode.Auth_InvalidProvider);
    }
}
