using RelationshipService.Domain.Base;
using RelationshipService.Domain.Enums;

namespace RelationshipService.Domain.Entities;

public class Swipe : Entity<int>
{
    public int SwiperUserId { get; private set; }

    public int SwipedUserId { get; private set; }

    public SwipeType IsLiked { get; private set; }  // superlike, like, dislike, supermessagelike
    
}