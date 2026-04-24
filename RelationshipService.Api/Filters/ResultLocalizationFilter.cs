using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using RelationshipService.Application.Localization;
using IResult = RelationshipService.Domain.Models.IResult;

namespace RelationshipService.Api.Filters;

public class ResultLocalizationFilter : IAsyncResultFilter
{
    private readonly IStringLocalizer<CommonResources> _localizer;

    public ResultLocalizationFilter(IStringLocalizer<CommonResources> localizer)
    {
        _localizer = localizer;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value is IResult result)
        {
            if (!result.IsSuccess)
            {
                // Set the HTTP Status Code automatically based on the Result object
                objectResult.StatusCode = (int)result.StatusCode;

                if (result.Error != null)
                {
                    // Localization logic
                    string localizedMessage = _localizer[result.Error.Code.ToString()];

                    if (!string.IsNullOrEmpty(localizedMessage) && localizedMessage != result.Error.Code.ToString())
                    {
                        // Create a new error with the localized description using 'with'
                        result.Error = result.Error with { Description = localizedMessage };
                    }
                }

                if (result.Errors != null && result.Errors.Any())
                {
                    for (int i = 0; i < result.Errors.Count; i++)
                    {
                        var localized = _localizer[result.Errors[i]];
                        if (!localized.ResourceNotFound)
                        {
                            result.Errors[i] = localized.Value;
                        }
                    }
                }
            }
        }

        await next();
    }
}
