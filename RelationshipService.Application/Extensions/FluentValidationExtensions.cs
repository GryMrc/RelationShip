using FluentValidation;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Extensions;

public static class FluentValidationExtensions
{
    /// <summary>
    /// Sets the validation error message using the ErrorCode enum name.
    /// This name is then used by the ResultLocalizationFilter to find the localized translation.
    /// </summary>
    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule, ErrorCode errorCode)
    {
        return rule.WithMessage(errorCode.ToString());
    }
}
