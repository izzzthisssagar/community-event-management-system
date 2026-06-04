using CommunityEventManagement.Domain.Entities;

namespace CommunityEventManagement.Domain.Interfaces;

/// <summary>
/// IVenueRepository is the contract for all database operations to do with venues.
/// </summary>
public interface IVenueRepository
{
    // Gets every venue.
    Task<List<Venue>> GetAllAsync();

    // Gets a single venue by Id, or null if not found.
    Task<Venue?> GetByIdAsync(Guid guidId);

    // Adds a new venue.
    Task AddAsync(Venue newVenue);

    // Updates an existing venue.
    Task UpdateAsync(Venue updatedVenue);

    // Deletes the venue with the given Id.
    Task DeleteAsync(Guid guidId);
}
