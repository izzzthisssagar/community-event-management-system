using CommunityEventManagement.Domain.Entities;

namespace CommunityEventManagement.Domain.Interfaces;

/// <summary>
/// IRegistrationRepository is the contract for all database operations to do with registrations
/// (the link between a participant and an event).
/// </summary>
public interface IRegistrationRepository
{
    // Gets a single registration by Id (including its Event and Participant), or null.
    Task<Registration?> GetByIdAsync(Guid guidId);

    // Gets all of the registrations made by one participant (their "my registrations" list).
    Task<List<Registration>> GetByParticipantAsync(Guid guidParticipantId);

    // Gets all of the registrations for one event (so an admin can see who is coming).
    Task<List<Registration>> GetByEventAsync(Guid guidEventId);

    // Adds a new registration.
    Task AddAsync(Registration newRegistration);

    // Updates an existing registration (for example after it has been cancelled).
    Task UpdateAsync(Registration updatedRegistration);

    // Deletes the registration with the given Id.
    Task DeleteAsync(Guid guidId);
}
