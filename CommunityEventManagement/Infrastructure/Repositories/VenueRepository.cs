using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Exceptions;
using CommunityEventManagement.Domain.Interfaces;
using CommunityEventManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CommunityEventManagement.Infrastructure.Repositories;

/// <summary>
/// VenueRepository is the concrete implementation of IVenueRepository.
/// </summary>
public class VenueRepository : IVenueRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _dcfContextFactory;

    public VenueRepository(IDbContextFactory<ApplicationDbContext> dcfContextFactory)
    {
        _dcfContextFactory = dcfContextFactory;
    }

    public async Task<List<Venue>> GetAllAsync()
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        return await context.Venues
            .AsNoTracking()
            .OrderBy(v => v.Name)
            .ToListAsync();
    }

    public async Task<Venue?> GetByIdAsync(Guid guidId)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        return await context.Venues
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == guidId);
    }

    public async Task AddAsync(Venue newVenue)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        context.Venues.Add(newVenue);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Venue updatedVenue)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        Venue? existingVenue = await context.Venues.FirstOrDefaultAsync(v => v.Id == updatedVenue.Id);
        if (existingVenue is null)
        {
            throw new EntityNotFoundException($"Could not update because venue with Id '{updatedVenue.Id}' was not found.");
        }

        existingVenue.Name = updatedVenue.Name;
        existingVenue.Address = updatedVenue.Address;
        existingVenue.Capacity = updatedVenue.Capacity;
        existingVenue.IsAccessible = updatedVenue.IsAccessible;
        existingVenue.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid guidId)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        Venue? venueToDelete = await context.Venues.FirstOrDefaultAsync(v => v.Id == guidId);
        if (venueToDelete is not null)
        {
            context.Venues.Remove(venueToDelete);
            await context.SaveChangesAsync();
        }
    }
}
