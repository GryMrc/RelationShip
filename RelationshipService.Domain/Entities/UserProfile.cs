using RelationshipService.Domain.Base;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Domain.Entities
{
    public class UserProfile : Entity<int>
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }
        public List<Hobby> Hobbies { get; set; }
        public Gender Gender { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public DateTime DateOfBirth { get; set; }
        public byte? Height { get; set; }
        public byte? Weight { get; set; }
        public string ZodiacSign { get; set; }
        public string RisingZodiacSign { get; set; }
        public Gender InterestedInGender { get; set; }
        public List<UserProfileAnswer> UserProfileAnswers { get; set; }
        public List<UserProfilePhoto> ProfilePhotos { get; set; }
        public sbyte MaxDistancePreference { get; set; }
        public byte MinAgePreference { get; set; }
        public byte MaxAgePreference { get; set; }
    }
}