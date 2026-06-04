using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Exceptions;
using CommunityEventManagement.Domain.Interfaces;
using CommunityEventManagement.Models.ViewModels;

namespace CommunityEventManagement.Application.Services;

/// <summary>
/// IVenueService is the contract for the venue business logic.
/// </summary>
public interface IVenueService
{
    Task<List<Venue>> GetAllAsync();
    Task<Venue> GetByIdAsync(Guid guidId);
    Task CreateAsync(VenueViewModel vmVenue);
    Task UpdateAsync(VenueViewModel vmVenue);
    Task DeleteAsync(Guid guidId);
}

/// <summary>
/// VenueService holds the logic for managing venues.
/// </summary>
public class VenueService : IVenueService
{
    private readonly IVenueRepository _vrVenueRepository;

    public VenueService(IVenueRepository vrVenueRepository)
    {
        _vrVenueRepository = vrVenueRepository;
    }

    public async Task<List<Venue>> GetAllAsync()
    {
        return await _vrVenueRepository.GetAllAsync();
    }

    public async Task<Venue> GetByIdAsync(Guid guidId)
    {
        Venue? foundVenue = await _vrVenueRepository.GetByIdAsync(guidId);
        if (foundVenue is null)
        {
            throw new EntityNotFoundException($"No venue was found with Id '{guidId}'.");
        }
        return foundVenue;
    }

    public async Task CreateAsync(VenueViewModel vmVenue)
    {
        Venue newVenue = new Venue(vmVenue.Name, vmVenue.Address, vmVenue.Capacity, vmVenue.IsAccessible);
        await _vrVenueRepository.AddAsync(newVenue);
    }

    public async Task UpdateAsync(VenueViewModel vmVenue)
    {
        if (vmVenue.Id is null)
        {
            throw new EventManagementException("Cannot update a venue without an Id.");
        }

        Venue existingVenue = await GetByIdAsync(vmVenue.Id.Value);
        existingVenue.Name = vmVenue.Name;
        existingVenue.Address = vmVenue.Address;
        existingVenue.Capacity = vmVenue.Capacity;
        existingVenue.IsAccessible = vmVenue.IsAccessible;

        await _vrVenueRepository.UpdateAsync(existingVenue);
    }

    public async Task DeleteAsync(Guid guidId)
    {
        await _vrVenueRepository.DeleteAsync(guidId);
    }
}
