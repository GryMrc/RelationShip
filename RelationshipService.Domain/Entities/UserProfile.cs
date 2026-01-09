namespace RelationshipService.Domain.Entities
{
    public class Profile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }
        public List<Hobby> Hobbies { get; set; }
        public GenderEnum Gender { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public DateTime DateOfBirth { get; set; }
        public byte? Height { get; set; }
        public byte? Weight { get; set; }
        public string ZodiacSign { get; set; }
        public string RisingZodiacSign { get; set; }
        public GenderEnum InterestedInGender { get; set; }
        public List<QuestionAnswer> QuestionAnswers { get; set; }
        public List<ProfilePhoto> ProfilePhotos { get; set; }

        public sbyte MaxDistancePreference { get; set; }
        public byte MinAgePreference { get; set; }
        public byte MaxAgePreference { get; set; }
    }
}