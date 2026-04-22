using FluentValidation;
using RelationshipService.Application.Models.Profile.Requests;

namespace RelationshipService.Application.Validators.Profile;

public class UpsertProfileDeviceRequestValidator : AbstractValidator<UpsertProfileDeviceRequest>
{
    public UpsertProfileDeviceRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Device token is required.");

        RuleFor(x => x.Platform)
            .NotEmpty()
            .WithMessage("Device platform is required.");

        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .WithMessage("Device ID is required.");

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Device name cannot exceed 100 characters.");
    }
}
