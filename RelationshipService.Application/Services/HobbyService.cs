using Microsoft.EntityFrameworkCore;
using RelationshipService.Application.Models.Hobby.Requests;
using RelationshipService.Application.Models.Hobby.Responses;
using RelationshipService.Application.ServiceContracts;
using RelationshipService.Domain.Entities;

namespace RelationshipService.Application.Services;

public class HobbyService(IRelationShipDbContext context) : IHobbyService
{
    public async Task<IEnumerable<HobbyResponse>> GetAllAsync()
    {
        return await context.Hobbies
            .AsNoTracking()
            .Select(h => new HobbyResponse
            {
                Id = h.Id,
                Name = h.Name
            })
            .ToListAsync();
    }

    public async Task<HobbyResponse?> GetByIdAsync(int id)
    {
        var hobby = await context.Hobbies.FindAsync(id);
        if (hobby == null) return null;

        return new HobbyResponse
        {
            Id = hobby.Id,
            Name = hobby.Name
        };
    }

    public async Task<HobbyResponse> CreateAsync(CreateHobbyRequest request)
    {
        var hobby = new Hobby
        {
            Name = request.Name
        };

        await context.Hobbies.AddAsync(hobby);
        await context.SaveChangesAsync();

        return new HobbyResponse
        {
            Id = hobby.Id,
            Name = hobby.Name
        };
    }

    public async Task<HobbyResponse?> UpdateAsync(UpdateHobbyRequest request)
    {
        var hobby = await context.Hobbies.FindAsync(request.Id);
        if (hobby == null) return null;

        hobby.Name = request.Name;
        
        context.Hobbies.Update(hobby);
        await context.SaveChangesAsync();

        return new HobbyResponse
        {
            Id = hobby.Id,
            Name = hobby.Name
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var hobby = await context.Hobbies.FindAsync(id);
        if (hobby == null) return false;

        context.Hobbies.Remove(hobby);
        await context.SaveChangesAsync();
        return true;
    }
}
