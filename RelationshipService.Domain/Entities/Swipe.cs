namespace RelationshipService.Domain.Entities;

public class Swipe
{
    public int Id { get; private set; }

    public int SwiperUserId { get; private set; }

    public int SwipedUserId { get; private set; }

    public ActionType IsLiked { get; private set; }  // superlike, like, dislike, supermessagelike

    public DateTime SwipedAt { get; private set; }

    public Swipe(int id, int swiperProfileId, int swipedProfileId, bool isLiked, DateTime swipedAt)
    {
        Id = id;
        SwiperProfileId = swiperProfileId;
        SwipedProfileId = swipedProfileId;
        IsLiked = isLiked;
        SwipedAt = swipedAt;
    }
}