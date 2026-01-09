namespace RelationshipService.Domain.Entities;
public enum GenderEnum
    {
        Male = 1,
        Famale = 2
    }

    public class Gender
    {
        public int Id { get; private set; }

        public string Name { get; private set; }

        private Gender(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public static readonly VerificationStatus Pending = 
            new((int)VerificationStatusEnum.Pending, VerificationStatusEnum.Pending.ToString());

        public static readonly VerificationStatus Expired =
            new((int)VerificationStatusEnum.Expired, VerificationStatusEnum.Expired.ToString());

        public static readonly VerificationStatus Verified =
            new((int)VerificationStatusEnum.Verified, VerificationStatusEnum.Verified.ToString());

        public static readonly VerificationStatus Failed =
            new((int)VerificationStatusEnum.Failed, VerificationStatusEnum.Failed.ToString());

        public static IEnumerable<GenderEnum> List() =>
            new[] { Male, Famale };

        public static VerificationStatus FromId(int id) =>
            List().SingleOrDefault(s => s.Id == id)
            ?? throw new ArgumentException($"Invalid id {id}");

        public static VerificationStatus FromEnum(VerificationStatusEnum statusEnum) =>
            FromId((int)statusEnum);
    }