using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Exceptions;
using CommunityEventManagement.Domain.Interfaces;
using CommunityEventManagement.Models.ViewModels;

namespace CommunityEventManagement.Application.Services;

/// <summary>
/// IParticipantService is the contract for the participant business logic.
/// </summary>
public interface IParticipantService
{
    Task<List<Participant>> GetAllAsync();
    Task<Participant> GetByIdAsync(Guid guidId);
    Task CreateAsync(ParticipantViewModel vmParticipant);
    Task UpdateAsync(ParticipantViewModel vmParticipant);
    Task DeleteAsync(Guid guidId);
}

/// <summary>
/// ParticipantService holds the logic for managing participants. It also checks for duplicate
/// email addresses before creating a participant, so the friendly error appears before the
/// database unique index would reject it.
/// </summary>
public class ParticipantService : IParticipantService
{
    private readonly IParticipantRepository _prParticipantRepository;

    public ParticipantService(IParticipantRepository prParticipantRepository)
    {
        _prParticipantRepository = prParticipantRepository;
    }

    public async Task<List<Participant>> GetAllAsync()
    {
        return await _prParticipantRepository.GetAllAsync();
    }

    public async Task<Participant> GetByIdAsync(Guid guidId)
    {
        Participant? foundParticipant = await _prParticipantRepository.GetByIdAsync(guidId);
        if (foundParticipant is null)
        {
            throw new EntityNotFoundException($"No participant was found with Id '{guidId}'.");
        }
        return foundParticipant;
    }

    public async Task CreateAsync(ParticipantViewModel vmParticipant)
    {
        // Make sure this email is not already used by another participant.
        Participant? duplicate = await _prParticipantRepository.GetByEmailAsync(vmParticipant.Email);
        if (duplicate is not null)
        {
            throw new EventManagementException($"A participant with the email '{vmParticipant.Email}' already exists.");
        }

        Participant newParticipant = new Participant(
            vmParticipant.FirstName,
            vmParticipant.LastName,
            vmParticipant.Email,
            vmParticipant.PhoneNumber);

        await _prParticipantRepository.AddAsync(newParticipant);
    }

    public async Task UpdateAsync(ParticipantViewModel vmParticipant)
    {
        if (vmParticipant.Id is null)
        {
            throw new EventManagementException("Cannot update a participant without an Id.");
        }

        // Load the existing participant so I keep its correct Id, then copy on the new values.
        Participant existingParticipant = await GetByIdAsync(vmParticipant.Id.Value);
        existingParticipant.FirstName = vmParticipant.FirstName;
        existingParticipant.LastName = vmParticipant.LastName;
        existingParticipant.Email = vmParticipant.Email;
        existingParticipant.PhoneNumber = vmParticipant.PhoneNumber;

        await _prParticipantRepository.UpdateAsync(existingParticipant);
    }

    public async Task DeleteAsync(Guid guidId)
    {
        await _prParticipantRepository.DeleteAsync(guidId);
    }
}
