using Microsoft.EntityFrameworkCore;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Application.Interfaces;

public interface IRelationShipDbContext
{
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserProfilePhoto> UserProfilePhotos { get; set; }
    public DbSet<UserLocationHistory> UserLocationHistories { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Swipe> Swipes { get; set; }
    public DbSet<Hobby> Hobbies { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionAnswer> QuestionAnswers { get; set; }
    public DbSet<UserProfileAnswer> UserProfileAnswers { get; set; }
    public DbSet<UserProfileHobby> UserProfileHobbies { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
