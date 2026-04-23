using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Localization;
using RelationshipService.Application.Localization;
using RelationshipService.Domain.Enums;
using RelationshipService.Domain.Models;
using System.Net;
using System.Text.Json;

namespace RelationshipService.Api.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IStringLocalizer<CommonResources> _localizer;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IStringLocalizer<CommonResources> localizer)
    {
        _logger = logger;
        _localizer = localizer;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var errorCode = ErrorCode.General_InternalServerError;
        var description = _localizer[errorCode.ToString()];

        var result = Result.Failure(new Error(errorCode, description), HttpStatusCode.InternalServerError);

        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(result, jsonOptions), cancellationToken);

        return true;
    }
}
