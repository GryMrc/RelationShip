using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Models.Profile.Requests;

public class CreateProfileRequest : ProfileRequestBase
{
    public string Name { get; set; }

    public Gender Gender { get; set; }

    public DateTime DateOfBirth { get; set; }
}
