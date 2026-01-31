using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Application;

public interface IRelationShipDbContext
{
    DbSet<UserProfile> UserProfiles { get; set; }
    DbSet<UserProfilePhoto> UserProfilePhotos { get; set; }
    DbSet<UserPreferences> UserPreferences { get; set; }
    DbSet<UserLocationHistory> UserLocationHistories { get; set; }
    DbSet<Match> Matches { get; set; }
    DbSet<Swipe> Swipes { get; set; }
    DbSet<Hobby> Hobbies { get; set; }
    DbSet<Question> Questions { get; set; }
    DbSet<QuestionAnswer> QuestionAnswers { get; set; }
    DbSet<UserProfileAnswer> UserProfileAnswers { get; set; }
    DbSet<UserProfileHobby> UserProfileHobbies { get; set; }

    ChangeTracker ChangeTracker { get;}
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
