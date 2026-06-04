using CommunityEventManagement.Domain.Entities;

namespace CommunityEventManagement.Domain.Interfaces;

/// <summary>
/// IParticipantRepository is the contract for all database operations to do with participants.
/// Following the Repository Pattern, the services depend on this interface and not on EF Core.
/// </summary>
public interface IParticipantRepository
{
    // Gets every participant.
    Task<List<Participant>> GetAllAsync();

    // Gets a single participant by Id, or null if not found.
    Task<Participant?> GetByIdAsync(Guid guidId);

    // Gets a participant by their email address. I use this to check for duplicate emails.
    Task<Participant?> GetByEmailAsync(string sEmail);

    // Adds a new participant.
    Task AddAsync(Participant newParticipant);

    // Updates an existing participant.
    Task UpdateAsync(Participant updatedParticipant);

    // Deletes the participant with the given Id.
    Task DeleteAsync(Guid guidId);
}
