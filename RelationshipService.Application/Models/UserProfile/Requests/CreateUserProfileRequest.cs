using RelationshipService.Domain.Enums;

namespace RelationshipService.Application.Models.UserProfile.Requests;

public class CreateUserProfileRequest : UserProfileRequestBase
{
    public string Name { get; set; }

    public Gender Gender { get; set; }

    public DateTime DateOfBirth { get; set; }
}
