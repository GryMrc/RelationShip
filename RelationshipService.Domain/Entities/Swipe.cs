using RelationshipService.Domain.Base;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Domain.Entities;

public class Swipe : Entity<int>
{
    private Swipe() { } // For EF Core

    public Swipe(int swiperUserId, int swipedUserId, SwipeType swipeType)
    {
        SwiperUserId = swiperUserId;
        SwipedUserId = swipedUserId;
        SwipeType = swipeType;
    }

    public int SwiperUserId { get; private set; }

    public int SwipedUserId { get; private set; }

    public SwipeType SwipeType { get; private set; }  // superlike, like, dislike, supermessagelike
}
