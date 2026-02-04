using RelationshipService.Domain.Base;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Domain.Entities;

public class Swipe : Entity<int>
{
    public long SwiperProfilId { get; set; }

    public long SwipedProfilId { get; set; }

    public SwipeType SwipeType { get; set; }

    public Mode Mode { get; set; }
}
