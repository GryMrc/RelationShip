using RelationshipService.Domain.Base;
using RelationshipService.Domain.Enums;
using NetTopologySuite.Geometries;

namespace RelationshipService.Domain.Entities
{
    public class UserProfile : Entity<long>
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string? Bio { get; set; }
        public List<Hobby> Hobbies { get; set; }
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public byte? Height { get; set; }
        public byte? Weight { get; set; }
        public Point Location { get; set; }
        public ZodiacSign? ZodiacSign { get; set; }
        public ZodiacSign? RisingZodiacSign { get; set; }
        public bool IsVerified { get; set; }
        public Mode Mode { get; set; } = Mode.Date;
        public SubscriptionPlan SubscriptionPlan { get; set; } = SubscriptionPlan.Free;





        public UserPreferences Preferences { get; set; }

        public List<UserProfileAnswer> UserProfileAnswers { get; set; }
        public List<UserProfilePhoto> ProfilePhotos { get; set; }
    }
}