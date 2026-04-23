using RelationshipService.Domain.Enums;

namespace RelationshipService.Domain.Models;

public record Error(ErrorCode Code, string? Description = null)
{
    public static readonly Error None = new(ErrorCode.None, string.Empty);
}

