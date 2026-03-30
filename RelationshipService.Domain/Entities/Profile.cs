using RelationshipService.Domain.Base;
using RelationshipService.Domain.Enums;
using NetTopologySuite.Geometries;

namespace RelationshipService.Domain.Entities
{
    public class Profile : Entity<long>
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string? Bio { get; set; }
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

        public ProfilePreferences Preferences { get; set; }
        //public virtual User User { get; set; } = null!;
        public List<Hobby> Hobbies { get; set; }
        public List<ProfileAnswer> Answers { get; set; }
        public List<ProfilePhoto> Photos { get; set; }
    }
}