using Microsoft.EntityFrameworkCore;
using RelationshipService.Application;
using RelationshipService.Domain.Base;
using RelationshipService.Domain.Entities;
using System.Reflection;

namespace RelationshipService.Infra;

public class RelationShipDbContext(DbContextOptions<RelationShipDbContext> options) : DbContext(options), IRelationShipDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Profile> UserProfiles { get; set; }
    public DbSet<ProfilePhoto> UserProfilePhotos { get; set; }
    public DbSet<ProfilePreferences> UserPreferences { get; set; }
    public DbSet<ProfileLocationHistory> UserLocationHistories { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Swipe> Swipes { get; set; }
    public DbSet<Hobby> Hobbies { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionAnswer> QuestionAnswers { get; set; }
    public DbSet<ProfileAnswer> UserProfileAnswers { get; set; }
    public DbSet<ProfileHobby> UserProfileHobbies { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RelationShipDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).Property(nameof(IEntity.IsDeleted)).HasColumnName("is_deleted");
                modelBuilder.Entity(entityType.ClrType).Property(nameof(IEntity.CreatedDate)).HasColumnName("created_date");
                modelBuilder.Entity(entityType.ClrType).Property(nameof(IEntity.UpdatedDate)).HasColumnName("updated_date");

                var method = typeof(RelationShipDbContext)
                    .GetMethod(nameof(SetGlobalQueryFilter), BindingFlags.NonPublic | BindingFlags.Static)
                    ?.MakeGenericMethod(entityType.ClrType);
                method?.Invoke(null, [modelBuilder]);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private static void SetGlobalQueryFilter<T>(ModelBuilder modelBuilder) where T : class, IEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
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
