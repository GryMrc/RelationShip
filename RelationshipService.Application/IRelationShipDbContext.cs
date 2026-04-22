using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Application;

public interface IRelationShipDbContext
{
    DbSet<User> Users { get; set; }
    DbSet<RefreshToken> RefreshTokens { get; set; }
    DbSet<Profile> UserProfiles { get; set; }
    DbSet<ProfilePhoto> UserProfilePhotos { get; set; }
    DbSet<ProfilePreferences> UserPreferences { get; set; }
    DbSet<ProfileLocationHistory> UserLocationHistories { get; set; }
    DbSet<Match> Matches { get; set; }
    DbSet<Swipe> Swipes { get; set; }
    DbSet<Hobby> Hobbies { get; set; }
    DbSet<Question> Questions { get; set; }
    DbSet<QuestionAnswer> QuestionAnswers { get; set; }
    DbSet<ProfileAnswer> UserProfileAnswers { get; set; }
    DbSet<ProfileHobby> UserProfileHobbies { get; set; }
    DbSet<Message> Messages { get; set; }
    DbSet<ProfileDevice> UserProfileDevices { get; set; }

    ChangeTracker ChangeTracker { get;}
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
