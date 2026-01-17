using Microsoft.EntityFrameworkCore;
using RelationshipService.Application;
using RelationshipService.Domain.Base;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Infra;

public class RelationShipDbContext(DbContextOptions<RelationShipDbContext> options) : DbContext(options), IRelationShipDbContext
{
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserProfilePhoto> UserProfilePhotos { get; set; }
    public DbSet<UserPreferences> UserPreferences { get; set; }
    public DbSet<UserLocation> UserLocations { get; set; }
    public DbSet<UserLocationHistory> UserLocationHistories { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Swipe> Swipes { get; set; }
    public DbSet<Hobby> Hobbies { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionAnswer> QuestionAnswers { get; set; }
    public DbSet<UserProfileAnswer> UserProfileAnswers { get; set; }
    public DbSet<UserProfileHobby> UserProfileHobbies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RelationShipDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<IEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedDate = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.UpdatedDate = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
