using RelationshipService.Domain.Base;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Domain.Entities;

public class Swipe : Entity<int>
{
    public long SwiperProfileId { get; set; }

    public long SwipedProfileId { get; set; }

    public SwipeType SwipeType { get; set; }

    public Mode Mode { get; set; }
}
